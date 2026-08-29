using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.LoyaltyAccounts.Commands.EarnPoints;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.LoyaltyAccount;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.LoyaltyAccounts;

public class EarnPointsEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/users/{userId:guid}/loyalty/earn",
                async (
                    Guid userId,
                    EarnPointsRequest request,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<EarnPointsEndpoint> logger
                ) =>
                {
                    var command = mapper.Map<EarnPointsCommand>((request, userId));

                    return await HandleAsync(
                        mediator,
                        command,
                        account => Results.Ok(mapper.Map<LoyaltyAccountResponse>(account)),
                        ex => logger.LoyaltyPointsEarnError(ex)
                    );
                }
            )
            .RequireAuthorization("Loyalty.Earn")
            .WithTags("LoyaltyAccounts")
            .WithSummary("Earn loyalty points")
            .WithDescription("Admin grants loyalty points to a user's account.")
            .Produces<LoyaltyAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
