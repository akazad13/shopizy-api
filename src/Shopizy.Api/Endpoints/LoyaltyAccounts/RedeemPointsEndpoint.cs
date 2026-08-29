using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.LoyaltyAccounts.Commands.RedeemPoints;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.LoyaltyAccount;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.LoyaltyAccounts;

public class RedeemPointsEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/users/{userId:guid}/loyalty/redeem",
                async (
                    Guid userId,
                    RedeemPointsRequest request,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<RedeemPointsEndpoint> logger
                ) =>
                {
                    if (user.AuthorizeOwner(userId, "this loyalty account") is { } forbidden)
                        return forbidden;

                    var command = mapper.Map<RedeemPointsCommand>((request, userId));

                    return await HandleAsync(
                        mediator,
                        command,
                        account => Results.Ok(mapper.Map<LoyaltyAccountResponse>(account)),
                        ex => logger.LoyaltyPointsRedeemError(ex)
                    );
                }
            )
            .RequireAuthorization("Loyalty.Redeem")
            .WithTags("LoyaltyAccounts")
            .WithSummary("Redeem loyalty points")
            .WithDescription(
                "Allows an authenticated user to redeem loyalty points from their account."
            )
            .Produces<LoyaltyAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
