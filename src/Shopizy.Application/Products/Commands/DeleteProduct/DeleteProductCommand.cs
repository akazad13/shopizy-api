using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid UserId, Guid ProductId)
    : ICommand<ErrorOr<Success>>,
        IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        [CacheKeys.Product(ProductId), CacheKeys.ProductVariants(ProductId)];
}
