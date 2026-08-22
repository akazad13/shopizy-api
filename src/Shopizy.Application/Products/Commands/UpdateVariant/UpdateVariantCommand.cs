using ErrorOr;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Products.Entities;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.UpdateVariant;

public record UpdateVariantCommand(
    Guid ProductId,
    Guid VariantId,
    string Name,
    string SKU,
    decimal UnitPrice,
    Currency Currency,
    int StockQuantity,
    bool IsActive
) : ICommand<ErrorOr<ProductVariant>>, IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        [$"product-variants:{ProductId}", $"product:{ProductId}"];
}
