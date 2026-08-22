using System.Text.Json.Serialization;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Carts.Entities;

public sealed class CartItem : Entity<CartItemId>
{
    public Product Product { get; private set; } = null!;
    public ProductId ProductId { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public string Size { get; private set; } = null!;
    public int Quantity { get; private set; }

    public static CartItem Create(ProductId productId, string color, string size, int quantity) =>
        new(CartItemId.CreateUnique(), productId, color, size, quantity);

    public void UpdateQuantity(int quantity) => Quantity = quantity;

    private CartItem(
        CartItemId CartItemId,
        ProductId productId,
        string color,
        string size,
        int quantity
    )
        : base(CartItemId)
    {
        ProductId = productId;
        Color = color;
        Size = size;
        Quantity = quantity;
    }

#pragma warning disable CS8618 // EF Core parameterless constructor
    [JsonConstructor]
    private CartItem() { }
#pragma warning restore CS8618
}
