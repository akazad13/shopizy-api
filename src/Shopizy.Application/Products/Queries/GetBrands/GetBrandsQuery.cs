using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Queries.GetBrands;

public record GetBrandsQuery() : IQuery<ErrorOr<IReadOnlyList<string>>>, ICachableRequest
{
    public string CacheKey => CacheKeys.ProductBrands;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(60);
}
