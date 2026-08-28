using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Returns.Commands.RejectReturn;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.Returns;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Returns;

public class RejectReturnEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut(
                "api/v1.0/returns/{returnId:guid}/reject",
                async (
                    Guid returnId,
                    [FromBody] RejectReturnRequest request,
                    [FromServices] IDispatcher mediator,
                    ILogger<RejectReturnEndpoint> logger
                ) =>
                {
                    var command = new RejectReturnCommand(returnId, request.AdminNote);

                    return await HandleAsync(
                        mediator,
                        command,
                        _ => Results.Ok(SuccessResult.Success("Return request rejected.")),
                        ex => logger.ReturnRejectionError(ex)
                    );
                }
            )
            .RequireAuthorization("Order.Manage")
            .WithTags("Returns")
            .WithSummary("Reject a return request (Admin)")
            .WithDescription("Rejects a pending return request with an admin note.")
            .Produces<SuccessResult>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
