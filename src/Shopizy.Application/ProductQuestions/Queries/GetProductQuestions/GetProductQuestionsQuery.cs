using ErrorOr;
using Shopizy.Domain.ProductQuestions;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.ProductQuestions.Queries.GetProductQuestions;

public record GetProductQuestionsQuery(Guid ProductId, int PageNumber, int PageSize)
    : IQuery<ErrorOr<IReadOnlyList<ProductQuestion>>>,
        ICachableRequest
{
    public string CacheKey => $"product-questions:{ProductId}:p{PageNumber}:s{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
