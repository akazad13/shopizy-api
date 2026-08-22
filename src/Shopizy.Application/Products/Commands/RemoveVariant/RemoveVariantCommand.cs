using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.RemoveVariant;

public record RemoveVariantCommand(Guid ProductId, Guid VariantId)
    : ICommand<ErrorOr<Deleted>>,
        IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        [$"product-variants:{ProductId}", $"product:{ProductId}"];
}
