using ErrorOr;
using Shopizy.Domain.Brands;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Brands.Queries.GetBrand;

public record GetBrandQuery(Guid BrandId) : IQuery<ErrorOr<Brand>>, ICachableRequest
{
    public string CacheKey => CacheKeys.Brand(BrandId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
