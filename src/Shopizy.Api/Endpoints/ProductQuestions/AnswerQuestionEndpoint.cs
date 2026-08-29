using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Common.Security.CurrentUser;
using Shopizy.Application.ProductQuestions.Commands.AnswerQuestion;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.ProductQuestion;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.ProductQuestions;

public class AnswerQuestionEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/admin/questions/{questionId:guid}/answer",
                async (
                    Guid questionId,
                    AnswerQuestionRequest request,
                    [FromServices] ICurrentUser currentUser,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<AnswerQuestionEndpoint> logger
                ) =>
                {
                    var answeredByUserId = currentUser.GetCurrentUserId();
                    var command = mapper.Map<AnswerQuestionCommand>(
                        (request, questionId, answeredByUserId)
                    );

                    return await HandleAsync(
                        mediator,
                        command,
                        question => Results.Ok(mapper.Map<ProductQuestionResponse>(question)),
                        ex => logger.ProductQuestionAnswerError(ex)
                    );
                }
            )
            .RequireAuthorization("Question.Answer")
            .WithTags("ProductQuestions")
            .WithSummary("Answer a product question")
            .WithDescription("Allows an admin to answer a product question.")
            .Produces<ProductQuestionResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status409Conflict)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
