using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.GiftCards.Commands.CreateGiftCard;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.GiftCard;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.GiftCards;

public class CreateGiftCardEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/admin/gift-cards",
                async (
                    CreateGiftCardRequest request,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<CreateGiftCardEndpoint> logger
                ) =>
                {
                    var command = mapper.Map<CreateGiftCardCommand>(request);

                    return await HandleAsync(
                        mediator,
                        command,
                        giftCard => Results.Ok(mapper.Map<GiftCardResponse>(giftCard)),
                        ex => logger.GiftCardCreationError(ex)
                    );
                }
            )
            .RequireAuthorization("GiftCard.Create")
            .WithTags("GiftCards")
            .WithSummary("Create gift card")
            .WithDescription("Creates a new gift card.")
            .Produces<GiftCardResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status409Conflict)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
