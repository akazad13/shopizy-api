using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.GiftCards.Queries.GetGiftCardById;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.GiftCard;
using Shopizy.Domain.GiftCards.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.GiftCards;

public class GetGiftCardByIdEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/admin/gift-cards/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetGiftCardByIdEndpoint> logger
                ) =>
                {
                    var query = new GetGiftCardByIdQuery(GiftCardId.Create(id));

                    return await HandleAsync(
                        mediator,
                        query,
                        giftCard => Results.Ok(mapper.Map<GiftCardResponse>(giftCard)),
                        ex => logger.GiftCardFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("GiftCard.Get")
            .WithTags("GiftCards")
            .WithSummary("Get gift card by ID")
            .WithDescription("Retrieves gift card details by its unique ID.")
            .Produces<GiftCardResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
