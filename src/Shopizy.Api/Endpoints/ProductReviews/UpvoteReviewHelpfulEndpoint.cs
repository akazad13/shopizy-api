using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.ProductReviews.Commands.UpvoteReviewHelpful;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.ProductReview;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.ProductReviews;

public class UpvoteReviewHelpfulEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "api/v1.0/products/{productId:guid}/reviews/{reviewId:guid}/helpful",
                async (
                    Guid productId,
                    Guid reviewId,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<UpvoteReviewHelpfulEndpoint> logger
                ) =>
                {
                    return await HandleAsync(
                        mediator,
                        new UpvoteReviewHelpfulCommand(reviewId),
                        review => Results.Ok(mapper.Map<ProductReviewResponse>(review)),
                        ex => logger.ProductReviewUpvoteError(ex)
                    );
                }
            )
            .AllowAnonymous()
            .WithTags("ProductReviews")
            .WithSummary("Upvote a product review as helpful")
            .WithDescription("Increments the helpful upvote count of a specific product review.")
            .Produces<ProductReviewResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
