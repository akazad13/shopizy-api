using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Application.Users.Commands.UpdateNotificationPreferences;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.User;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Users;

public class UpdateNotificationPreferencesEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut(
                "api/v1.0/users/{userId:guid}/notification-preferences",
                async (
                    Guid userId,
                    UpdateNotificationPreferencesRequest request,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<UpdateNotificationPreferencesEndpoint> logger
                ) =>
                {
                    if (
                        user.AuthorizeOwner(userId, "this user's notification preferences") is
                        { } forbidden
                    )
                        return forbidden;

                    var command = mapper.Map<UpdateNotificationPreferencesCommand>(
                        (userId, request)
                    );

                    return await HandleAsync(
                        mediator,
                        command,
                        (NotificationPreferencesDto dto) =>
                            Results.Ok(mapper.Map<NotificationPreferencesResponse>((userId, dto))),
                        ex => logger.NotificationPreferencesUpdateError(ex)
                    );
                }
            )
            .RequireAuthorization("User.Update")
            .WithTags("Users")
            .WithSummary("Update user notification preferences")
            .WithDescription(
                "Updates the multi-channel notification preferences for the specified user."
            )
            .Produces<NotificationPreferencesResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
