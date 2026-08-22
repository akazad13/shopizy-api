using System.Text.Json.Serialization;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products.Entities;

public sealed class ProductVariant : Entity<ProductVariantId>
{
    public string Name { get; private set; } = null!;
    public string SKU { get; private set; } = null!;
    public Price UnitPrice { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    public static ProductVariant Create(
        string name,
        string sku,
        Price unitPrice,
        int stockQuantity
    ) => new(ProductVariantId.CreateUnique(), name, sku, unitPrice, stockQuantity, isActive: true);

    public void Update(string name, string sku, Price unitPrice, int stockQuantity, bool isActive)
    {
        Name = name;
        SKU = sku;
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    private ProductVariant(
        ProductVariantId id,
        string name,
        string sku,
        Price unitPrice,
        int stockQuantity,
        bool isActive
    )
        : base(id)
    {
        Name = name;
        SKU = sku;
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    [JsonConstructor]
    private ProductVariant() { }
}
