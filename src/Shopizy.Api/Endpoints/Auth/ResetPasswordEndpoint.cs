using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Users.Commands.ResetPassword;
using Shopizy.Contracts.Authentication;
using Shopizy.Contracts.Common;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Auth;

public class ResetPasswordEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/auth/reset-password",
                async (
                    ResetPasswordRequest request,
                    [FromServices] IDispatcher mediator,
                    ILogger<ResetPasswordEndpoint> logger
                ) =>
                {
                    var command = new ResetPasswordCommand(request.ResetToken, request.NewPassword);

                    return await HandleAsync(
                        mediator,
                        command,
                        _ =>
                            Results.Ok(
                                SuccessResult.Success("Password has been reset successfully.")
                            ),
                        ex => logger.UserPasswordUpdateError(ex)
                    );
                }
            )
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithTags("Auth")
            .WithSummary("Reset password")
            .WithDescription("Resets the user's password using a valid reset token.")
            .Produces<SuccessResult>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
