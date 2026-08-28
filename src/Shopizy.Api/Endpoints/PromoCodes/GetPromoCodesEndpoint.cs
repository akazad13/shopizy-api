using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.PromoCodes.Queries.GetPromoCodes;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.PromoCode;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.PromoCodes;

public class GetPromoCodesEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/admin/promo-codes",
                async (
                    [FromQuery] int pageNumber,
                    [FromQuery] int pageSize,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetPromoCodesEndpoint> logger
                ) =>
                {
                    return await HandleAsync(
                        mediator,
                        new GetPromoCodesQuery(
                            pageNumber <= 0 ? 1 : pageNumber,
                            pageSize <= 0 ? 10 : pageSize
                        ),
                        promoCodes => Results.Ok(mapper.Map<List<PromoCodeResponse>>(promoCodes)),
                        ex => logger.PromoCodeFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Admin.PromoCode.Get")
            .WithTags("PromoCodes")
            .WithSummary("List all promo codes")
            .WithDescription("Returns all promotional codes.")
            .Produces<List<PromoCodeResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
