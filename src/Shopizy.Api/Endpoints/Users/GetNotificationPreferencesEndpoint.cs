using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Application.Users.Queries.GetNotificationPreferences;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.User;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Users;

public class GetNotificationPreferencesEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/users/{userId:guid}/notification-preferences",
                async (
                    Guid userId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetNotificationPreferencesEndpoint> logger
                ) =>
                {
                    if (
                        user.AuthorizeOwner(userId, "this user's notification preferences") is
                        { } forbidden
                    )
                        return forbidden;

                    var query = new GetNotificationPreferencesQuery(userId);

                    return await HandleAsync(
                        mediator,
                        query,
                        (NotificationPreferencesDto dto) =>
                            Results.Ok(mapper.Map<NotificationPreferencesResponse>((userId, dto))),
                        ex => logger.NotificationPreferencesFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("User.Get")
            .WithTags("Users")
            .WithSummary("Get user notification preferences")
            .WithDescription(
                "Retrieves the multi-channel notification preferences for the specified user."
            )
            .Produces<NotificationPreferencesResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
