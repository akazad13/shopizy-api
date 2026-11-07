using System.Diagnostics.CodeAnalysis;
using ErrorOr;
using Microsoft.Extensions.Options;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Models;
using Stripe;

namespace Shopizy.Infrastructure.ExternalServices.PaymentGateway.Stripe;

/// <summary>
/// Service for integrating with Stripe payment gateway.
/// </summary>
/// <param name="customerService"></param>
/// <param name="paymentIntentService"></param>
/// <param name="refundService"></param>
/// <param name="stripeSettings"></param>
[ExcludeFromCodeCoverage]
public class StripeService(
    CustomerService customerService,
    PaymentIntentService paymentIntentService,
    RefundService refundService,
    IOptions<StripeSettings> stripeSettings
) : IPaymentService
{
    private readonly CustomerService _customerService = customerService;
    private readonly PaymentIntentService _paymentIntentService = paymentIntentService;
    private readonly RefundService _refundService = refundService;
    private readonly StripeSettings _stripeSettings = stripeSettings.Value;

    private static bool IsTransientStripeError(StripeException ex) =>
        ex.StripeError?.Code == "rate_limit_error"
        || ex.StripeError?.Code == "api_connection_error";

    /// <summary>
    /// Creates a new customer in Stripe.
    /// </summary>
    /// <param name="email">The customer's email address.</param>
    /// <param name="name">The customer's name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A customer resource if successful; otherwise, an error.</returns>
    public async Task<ErrorOr<CustomerResource>> CreateCustomer(
        string email,
        string name,
        CancellationToken cancellationToken
    )
    {
        var maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var customerOptions = new CustomerCreateOptions { Email = email, Name = name };
                Customer customer = await _customerService.CreateAsync(
                    options: customerOptions,
                    requestOptions: null,
                    cancellationToken: cancellationToken
                );

                return new CustomerResource(customer.Id, customer.Email, customer.Name);
            }
            catch (StripeException ex)
                when (IsTransientStripeError(ex) && attempt < maxAttempts - 1)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt)),
                    cancellationToken
                );
            }
            catch (StripeException ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        return Error.Failure(description: "Stripe request failed after maximum retry attempts.");
    }

    /// <summary>
    /// Creates a payment sale (payment intent) in Stripe.
    /// </summary>
    /// <param name="request">The sale creation request.</param>
    /// <returns>A sale response if successful; otherwise, an error.</returns>
    public async Task<ErrorOr<CreateSaleResponse>> CreateSaleAsync(CreateSaleRequest request)
    {
        var intentCreateOptions = new PaymentIntentCreateOptions
        {
            Customer = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            // ConfirmationMethod = "manual",  // if Confirm = false, then this will determine how a payment will be confirmed (From frontend/backend)
            Confirm = request.CapturePayment,
            AllowedPaymentMethodTypes = request.PaymentMethodTypes?.ToList(),
            Metadata = request.Metadata,
            PaymentMethod = request.PaymentMethodId,
        };

        var maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await _paymentIntentService.CreateAsync(intentCreateOptions);

                // var result = await _paymentIntentService.ConfirmAsync(response.Id); // if Confirm is false and we manaully confirm payment

                return new CreateSaleResponse
                {
                    ResponseStatusCode = (int)response.StripeResponse.StatusCode,
                    Amount = response.Amount,
                    Currency = response.Currency,
                    PaymentIntentId = response.Id,
                    ObjectType = response.Object,
                    PaymentMethodId = response.PaymentMethodId,
                    CaptureMethod = response.CaptureMethod,
                    CustomerId = response.CustomerId,
                    ChargeId = response.LatestChargeId,
                    Status = response.Status,
                    Metadata = response.Metadata,
                    PaymentMethodTypes = response.PaymentMethodTypes,
                };
            }
            catch (StripeException ex)
                when (IsTransientStripeError(ex) && attempt < maxAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt)));
            }
            catch (StripeException ex)
            {
                return Error.Failure(
                    code: ex.StripeResponse.StatusCode.ToString(),
                    description: FormatStripeException(ex)
                );
            }
            catch (Exception ex)
            {
                return Error.Failure(code: "500", description: ex.Message);
            }
        }

        return Error.Failure(
            code: "500",
            description: "Stripe request failed after maximum retry attempts."
        );
    }

    /// <summary>
    /// Issues a full refund for a Stripe charge.
    /// </summary>
    /// <param name="chargeId">The Stripe charge ID to refund.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if the refund was created; otherwise, an error.</returns>
    public async Task<ErrorOr<Success>> CreateRefundAsync(
        string chargeId,
        CancellationToken cancellationToken
    )
    {
        var maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var options = new RefundCreateOptions { Charge = chargeId };
                await _refundService.CreateAsync(options, cancellationToken: cancellationToken);
                return Result.Success;
            }
            catch (StripeException ex)
                when (IsTransientStripeError(ex) && attempt < maxAttempts - 1)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt)),
                    cancellationToken
                );
            }
            catch (StripeException ex)
            {
                return Error.Failure(
                    code: ex.StripeResponse.StatusCode.ToString(),
                    description: FormatStripeException(ex)
                );
            }
        }

        return Error.Failure(
            code: "500",
            description: "Stripe refund failed after maximum retry attempts."
        );
    }

    /// <summary>
    /// Validates and parses an incoming Stripe webhook event.
    /// </summary>
    /// <param name="jsonPayload">The raw JSON payload.</param>
    /// <param name="signatureHeader">The Stripe-Signature header value.</param>
    /// <returns>A normalized <see cref="PaymentWebhookEvent"/> or an error.</returns>
    public ErrorOr<PaymentWebhookEvent> ParseWebhookEvent(
        string jsonPayload,
        string signatureHeader
    )
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                jsonPayload,
                signatureHeader,
                _stripeSettings.WebhookSecret,
                throwOnApiVersionMismatch: false
            );

            return stripeEvent.Type switch
            {
                EventTypes.PaymentIntentSucceeded => ExtractFromPaymentIntent(
                    stripeEvent.Data.Object as PaymentIntent,
                    PaymentWebhookEventType.PaymentSucceeded
                ),
                EventTypes.PaymentIntentPaymentFailed => ExtractFromPaymentIntent(
                    stripeEvent.Data.Object as PaymentIntent,
                    PaymentWebhookEventType.PaymentFailed
                ),
                EventTypes.ChargeSucceeded => ExtractFromCharge(
                    stripeEvent.Data.Object as Charge,
                    PaymentWebhookEventType.PaymentSucceeded
                ),
                EventTypes.ChargeRefunded => ExtractFromCharge(
                    stripeEvent.Data.Object as Charge,
                    PaymentWebhookEventType.ChargeRefunded
                ),
                _ => new PaymentWebhookEvent(
                    PaymentWebhookEventType.Unknown,
                    null,
                    null,
                    null,
                    null,
                    null
                ),
            };
        }
        catch (StripeException ex)
        {
            return Error.Validation(
                code: "stripe.webhook_signature_invalid",
                description: ex.Message
            );
        }
        catch (Exception ex)
        {
            return Error.Failure(code: "stripe.webhook_processing_error", description: ex.Message);
        }
    }

    private static PaymentWebhookEvent ExtractFromPaymentIntent(
        PaymentIntent? intent,
        PaymentWebhookEventType eventType
    )
    {
        if (intent is null)
        {
            return new PaymentWebhookEvent(
                PaymentWebhookEventType.Unknown,
                null,
                null,
                null,
                null,
                null
            );
        }

        string? orderId = null;
        if (intent.Metadata != null && intent.Metadata.TryGetValue("OrderId", out var value))
        {
            orderId = value;
        }

        return new PaymentWebhookEvent(
            eventType,
            orderId,
            intent.LatestChargeId,
            intent.CustomerId,
            intent.Id,
            intent.LastPaymentError?.Message
        );
    }

    private static PaymentWebhookEvent ExtractFromCharge(
        Charge? charge,
        PaymentWebhookEventType eventType
    )
    {
        if (charge is null)
        {
            return new PaymentWebhookEvent(
                PaymentWebhookEventType.Unknown,
                null,
                null,
                null,
                null,
                null
            );
        }

        string? orderId = null;
        if (charge.Metadata != null && charge.Metadata.TryGetValue("OrderId", out var value))
        {
            orderId = value;
        }

        return new PaymentWebhookEvent(
            eventType,
            orderId,
            charge.Id,
            charge.CustomerId,
            charge.PaymentIntentId,
            charge.FailureMessage
        );
    }

    private static string FormatStripeException(StripeException e) =>
        e.StripeError.Type switch
        {
            "card_error" => $"A payment error occurred: {e.StripeError.Message}",
            "api_connection_error" =>
                $"An error occurred while trying to connect to the stripe API: ${e.StripeError.Message}",
            "api_error" => $"An API error occurred: {e.StripeError.Message}",
            "authentication_error" =>
                $"An error occurred authenticating to Stripe API: {e.StripeError.Message}",
            "invalid_request_error" => $"An invalid request occurred: {e.StripeError.Message}",
            "rate_limit_error" => $"A rate limit error occurred: {e.StripeError.Message}",
            "validation_error" => $"A validation error occurred: {e.StripeError.Message}",
            _ => $"An unknown error occured: {e.StripeError.Message}",
        };
}
