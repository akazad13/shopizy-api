using ErrorOr;
using Shopizy.Domain.Brands;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Brands.Commands.CreateBrand;

public record CreateBrandCommand(Guid UserId, string Name, string? LogoUrl, string Country)
    : ICommand<ErrorOr<Brand>>,
        IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        ["brands-all", "brands-string-list"];
}
