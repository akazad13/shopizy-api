using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Brands.Queries.ListBrands;

public record ListBrandsQuery() : IQuery<ErrorOr<List<BrandItem>>>, ICachableRequest
{
    public string CacheKey => CacheKeys.BrandsAll;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
