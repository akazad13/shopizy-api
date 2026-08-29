using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.GiftCards.Queries.GetGiftCards;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.GiftCard;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.GiftCards;

public class GetGiftCardsEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/admin/gift-cards",
                async (
                    [FromQuery] int pageNumber,
                    [FromQuery] int pageSize,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetGiftCardsEndpoint> logger
                ) =>
                {
                    return await HandleAsync(
                        mediator,
                        new GetGiftCardsQuery(pageNumber, pageSize),
                        giftCards =>
                            Results.Ok(mapper.Map<IReadOnlyList<GiftCardResponse>>(giftCards)),
                        ex => logger.GiftCardFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("GiftCard.Get")
            .WithTags("GiftCards")
            .WithSummary("Get gift cards")
            .WithDescription("Returns a paginated list of all gift cards.")
            .Produces<IReadOnlyList<GiftCardResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
