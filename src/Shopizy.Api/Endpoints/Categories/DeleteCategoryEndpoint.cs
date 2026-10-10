using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Categories.Commands.DeleteCategory;
using Shopizy.Application.Common.Security.CurrentUser;
using Shopizy.Contracts.Common;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Categories;

public class DeleteCategoryEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete(
                "api/v1.0/admin/categories/{categoryId:guid}",
                async (
                    Guid categoryId,
                    [FromServices] ICurrentUser currentUser,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<DeleteCategoryEndpoint> logger
                ) =>
                {
                    var command = mapper.Map<DeleteCategoryCommand>(
                        (currentUser.GetCurrentUserId(), categoryId)
                    );

                    return await HandleAsync(
                        mediator,
                        command,
                        success =>
                            Results.Ok(SuccessResult.Success("Successfully deleted category.")),
                        ex => logger.CategoryDeleteError(ex)
                    );
                }
            )
            .RequireAuthorization("Category.Delete")
            .WithTags("Categories")
            .WithSummary("Delete category")
            .WithDescription("Deletes an existing product category.")
            .Produces<SuccessResult>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status409Conflict)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
