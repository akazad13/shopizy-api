namespace Shopizy.Contracts.Product;

/// <summary>
/// Represents a request to create a new product.
/// </summary>
/// <param name="Name">The name of the product.</param>
/// <param name="ShortDescription">A short description of the product.</param>
/// <param name="Description">The full description of the product.</param>
/// <param name="CategoryId">The category identifier.</param>
/// <param name="UnitPrice">The unit price.</param>
/// <param name="Currency">The currency code (as integer).</param>
/// <param name="Discount">The discount amount.</param>
/// <param name="Sku">The stock keeping unit.</param>
/// <param name="BrandId">The brand identifier.</param>
/// <param name="Colors">Comma-separated list of colors.</param>
/// <param name="Sizes">Comma-separated list of sizes.</param>
/// <param name="Tags">Comma-separated list of tags.</param>
/// <param name="Barcode">The barcode.</param>
/// <param name="StockQuantity">The initial stock quantity.</param>
/// <param name="SpecificationIds">List of specification identifiers.</param>
/// <param name="Highlights">Bullet-point product highlights.</param>
public record CreateProductRequest(
    string Name,
    string ShortDescription,
    string Description,
    Guid CategoryId,
    decimal UnitPrice,
    int Currency,
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
);
