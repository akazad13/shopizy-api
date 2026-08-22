using ErrorOr;
using Shopizy.Domain.ProductReviews;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.ProductReviews.Queries.GetProductReviews;

public record GetProductReviewsQuery(Guid ProductId, int PageNumber, int PageSize)
    : IQuery<ErrorOr<List<ProductReview>>>,
        ICachableRequest
{
    public string CacheKey => $"product-reviews:{ProductId}:p{PageNumber}:s{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
