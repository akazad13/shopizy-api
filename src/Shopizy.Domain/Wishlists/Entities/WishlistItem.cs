using System.Text.Json.Serialization;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Wishlists.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Wishlists.Entities;

public sealed class WishlistItem : Entity<WishlistItemId>
{
    public ProductId ProductId { get; private set; } = null!;

    public static WishlistItem Create(ProductId productId) =>
        new(WishlistItemId.CreateUnique(), productId);

    private WishlistItem(WishlistItemId id, ProductId productId)
        : base(id)
    {
        ProductId = productId;
    }

    [JsonConstructor]
    private WishlistItem() { }
}
