using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Notifications.Commands.SendEmail;
using Shopizy.Contracts.Common;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Notifications;

public record SendEmailRequest(string To, string Subject, string Body);

public class SendEmailNotificationEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/notifications/email",
                async (
                    [FromBody] SendEmailRequest request,
                    [FromServices] IDispatcher mediator,
                    ILogger<SendEmailNotificationEndpoint> logger
                ) =>
                {
                    var command = new SendEmailCommand(request.To, request.Subject, request.Body);

                    return await HandleAsync(
                        mediator,
                        command,
                        success => Results.Ok(new { Success = success }),
                        ex => logger.EmailDispatchError(ex)
                    );
                }
            )
            .RequireAuthorization("Admin")
            .WithTags("Notifications")
            .WithSummary("Send email notification")
            .WithDescription("Dispatches a transactional or alert email to a recipient.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
