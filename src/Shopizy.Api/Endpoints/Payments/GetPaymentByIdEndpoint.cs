using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Payments.Queries.GetPaymentById;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.Payment;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Payments;

public class GetPaymentByIdEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/payments/{paymentId:guid}",
                async (
                    Guid paymentId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetPaymentByIdEndpoint> logger
                ) =>
                {
                    var query = new GetPaymentByIdQuery(paymentId);

                    return await HandleAsync(
                        mediator,
                        query,
                        payment =>
                        {
                            if (!user.IsInRole("Admin") && !user.IsAuthorized(payment.UserId.Value))
                            {
                                return CustomResults.Problem([
                                    ErrorOr.Error.Forbidden(
                                        description: "You are not authorized to view this payment."
                                    ),
                                ]);
                            }

                            return Results.Ok(mapper.Map<PaymentDto>(payment));
                        },
                        ex => logger.PaymentFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Order.Read") // Ideally Payment.Read or Order.Read
            .WithTags("Payments")
            .WithSummary("Get a specific payment by ID")
            .WithDescription("Retrieves the details of a specific payment by its unique ID.")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
