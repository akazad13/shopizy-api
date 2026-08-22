using ErrorOr;
using Shopizy.Domain.Products.Entities;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Queries.GetProductVariants;

public record GetProductVariantsQuery(Guid ProductId)
    : IQuery<ErrorOr<IReadOnlyList<ProductVariant>>>,
        ICachableRequest
{
    public string CacheKey => $"product-variants:{ProductId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}
