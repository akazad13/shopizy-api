using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Brands.Commands.UpdateBrand;

public record UpdateBrandCommand(
    Guid UserId,
    Guid BrandId,
    string Name,
    string? LogoUrl,
    string Country
) : ICommand<ErrorOr<Success>>, IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        [CacheKeys.BrandsAll, CacheKeys.ProductBrands, CacheKeys.Brand(BrandId)];
}
