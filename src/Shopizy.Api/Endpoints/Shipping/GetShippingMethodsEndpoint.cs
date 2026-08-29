using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Application.Shipping.Queries.GetShippingMethods;
using Shopizy.Contracts.Common;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Shipping;

public class GetShippingMethodsEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/shipping/methods",
                async (
                    [FromServices] IDispatcher mediator,
                    ILogger<GetShippingMethodsEndpoint> logger
                ) =>
                {
                    var query = new GetShippingMethodsQuery();

                    return await HandleAsync(
                        mediator,
                        query,
                        methods => Results.Ok(methods),
                        ex => logger.ShippingRateEstimationError(ex)
                    );
                }
            )
            .AllowAnonymous()
            .WithTags("Shipping")
            .WithSummary("Get shipping methods")
            .WithDescription(
                "Retrieves available fixed shipping methods (Free, Standard, Express) and delivery timeframes."
            )
            .Produces<IReadOnlyList<ShippingRateEstimateDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
