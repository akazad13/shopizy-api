using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Common.Security.CurrentUser;
using Shopizy.Application.ProductQuestions.Commands.AskQuestion;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.ProductQuestion;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.ProductQuestions;

public class AskQuestionEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/products/{productId:guid}/questions",
                async (
                    Guid productId,
                    AskQuestionRequest request,
                    [FromServices] ICurrentUser currentUser,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<AskQuestionEndpoint> logger
                ) =>
                {
                    var userId = currentUser.GetCurrentUserId();
                    var command = mapper.Map<AskQuestionCommand>((request, userId, productId));

                    return await HandleAsync(
                        mediator,
                        command,
                        question => Results.Ok(mapper.Map<ProductQuestionResponse>(question)),
                        ex => logger.ProductQuestionCreationError(ex)
                    );
                }
            )
            .RequireAuthorization("Question.Ask")
            .WithTags("ProductQuestions")
            .WithSummary("Ask a product question")
            .WithDescription("Allows an authenticated user to ask a question about a product.")
            .Produces<ProductQuestionResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
