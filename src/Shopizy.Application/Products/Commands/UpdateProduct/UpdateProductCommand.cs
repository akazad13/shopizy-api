using ErrorOr;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid UserId,
    Guid ProductId,
    string Name,
    string ShortDescription,
    string Description,
    CategoryId CategoryId,
    Price UnitPrice,
    decimal Discount,
    string Sku,
    BrandId? BrandId,
    string Colors,
    string Sizes,
    string Tags,
    string Barcode,
    int StockQuantity,
    IList<Guid>? SpecificationIds,
    string? Highlights = null
) : ICommand<ErrorOr<Success>>, IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CacheKeys.Product(ProductId)];
}
