using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.LoyaltyAccounts.Queries.GetLoyaltyAccount;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.LoyaltyAccount;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.LoyaltyAccounts;

public class GetLoyaltyAccountEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/users/{userId:guid}/loyalty",
                async (
                    Guid userId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetLoyaltyAccountEndpoint> logger
                ) =>
                {
                    if (user.AuthorizeOwner(userId, "this loyalty account") is { } forbidden)
                        return forbidden;

                    return await HandleAsync(
                        mediator,
                        new GetLoyaltyAccountQuery(userId),
                        account => Results.Ok(mapper.Map<LoyaltyAccountResponse>(account)),
                        ex => logger.LoyaltyAccountFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Loyalty.Get")
            .WithTags("LoyaltyAccounts")
            .WithSummary("Get loyalty account")
            .WithDescription("Returns the loyalty account and transaction history for a user.")
            .Produces<LoyaltyAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
