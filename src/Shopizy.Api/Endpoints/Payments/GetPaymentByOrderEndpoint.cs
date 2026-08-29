using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Payments.Queries.GetPaymentByOrder;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.Payment;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Payments;

public class GetPaymentByOrderEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/orders/{orderId:guid}/payments",
                async (
                    Guid orderId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetPaymentByOrderEndpoint> logger
                ) =>
                {
                    var query = new GetPaymentByOrderQuery(orderId);

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
            .RequireAuthorization("Order.Read")
            .WithTags("Payments")
            .WithSummary("Get payment by Order ID")
            .WithDescription("Retrieves the payment associated with a specific order ID.")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
