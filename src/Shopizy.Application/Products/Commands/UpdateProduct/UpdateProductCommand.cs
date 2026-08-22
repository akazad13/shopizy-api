using ErrorOr;
using Shopizy.Domain.Common.Enums;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid UserId,
    Guid ProductId,
    string Name,
    string ShortDescription,
    string Description,
    Guid CategoryId,
    decimal UnitPrice,
    Currency Currency,
    decimal Discount,
    string Sku,
    Guid? BrandId,
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
