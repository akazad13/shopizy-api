using System.Text.Json.Serialization;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.ProductReviews;
using Shopizy.Domain.Products.Entities;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public sealed class Product : AggregateRoot<ProductId, Guid>, IAuditable
{
    [JsonInclude]
    private List<ProductImage> _productImages = [];

    [JsonInclude]
    private List<ProductReview> _productReviews = [];

    [JsonInclude]
    private List<ProductVariant> _productVariants = [];

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the short description of the product.
    /// </summary>
    public string ShortDescription { get; private set; } = null!;

    /// <summary>
    /// Gets the detailed description of the product.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets key bullet-point highlights of the product.
    /// </summary>
    public string? Highlights { get; private set; }

    /// <summary>
    /// Gets the category ID this product belongs to.
    /// </summary>
    public CategoryId CategoryId { get; private set; } = null!;

    /// <summary>
    /// Gets the stock keeping unit (SKU).
    /// </summary>
    public string SKU { get; private set; } = null!;

    /// <summary>
    /// Gets the current stock quantity.
    /// </summary>
    public int StockQuantity { get; private set; }

    /// <summary>
    /// Gets the unit price of the product.
    /// </summary>
    public Price UnitPrice { get; private set; } = null!;

    /// <summary>
    /// Gets the discount percentage.
    /// </summary>
    public decimal? Discount { get; private set; }

    /// <summary>
    /// Gets the product brand identifier.
    /// </summary>
    public BrandId? BrandId { get; private set; }

    /// <summary>
    /// Gets the available colors (comma-separated).
    /// </summary>
    public string Colors { get; private set; } = null!;

    /// <summary>
    /// Gets the available sizes (comma-separated).
    /// </summary>
    public string Sizes { get; private set; } = null!;

    /// <summary>
    /// Gets the number of times this product has been favorited.
    /// </summary>
    public int Favourites { get; private set; }

    /// <summary>
    /// Gets the product barcode.
    /// </summary>
    public string Barcode { get; private set; } = null!;

    /// <summary>
    /// Gets the product tags (comma-separated).
    /// </summary>
    public string Tags { get; private set; } = null!;

    /// <summary>
    /// Gets the average rating of the product.
    /// </summary>
    public AverageRating AverageRating { get; private set; } = null!;

    /// <summary>
    /// Gets whether the product is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the date and time when the product was created.
    /// </summary>
    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Gets the date and time when the product was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; private set; }

    /// <summary>
    /// Gets the read-only list of product images.
    /// </summary>
    public IReadOnlyList<ProductImage> ProductImages => (_productImages ?? []).AsReadOnly();

    /// <summary>
    /// Gets the read-only list of product reviews.
    /// </summary>
    public IReadOnlyList<ProductReview> ProductReviews => (_productReviews ?? []).AsReadOnly();

    /// <summary>
    /// Gets the read-only list of product variants.
    /// </summary>
    public IReadOnlyList<ProductVariant> ProductVariants => (_productVariants ?? []).AsReadOnly();

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="shortDescription">A brief description.</param>
    /// <param name="description">The full product description.</param>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="sku">The stock keeping unit.</param>
    /// <param name="stockQuantity">The initial stock quantity.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="discount">The discount percentage.</param>
    /// <param name="brandId">The brand identifier.</param>
    /// <param name="barcode">The product barcode.</param>
    /// <param name="colors">Available colors.</param>
    /// <param name="sizes">Available sizes.</param>
    /// <param name="tags">Product tags.</param>
    /// <param name="highlights">Bullet-point product highlights.</param>
    /// <returns>A new <see cref="Product"/> instance.</returns>
    public static Product Create(
        string name,
        string shortDescription,
        string description,
        CategoryId categoryId,
        string sku,
        int stockQuantity,
        Price unitPrice,
        decimal? discount,
        BrandId? brandId,
        string barcode,
        string colors,
        string sizes,
        string tags,
        string? highlights = null
    )
    {
        var product = new Product(
            ProductId.CreateUnique(),
            name,
            shortDescription,
            description,
            categoryId,
            sku,
            stockQuantity,
            unitPrice,
            discount,
            brandId,
            barcode,
            colors,
            sizes,
            tags,
            AverageRating.CreateNew(0),
            highlights
        );

        product.AddDomainEvent(new Events.ProductCreatedDomainEvent(product));

        return product;
    }

    /// <summary>
    /// Updates the product information.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="shortDescription">A brief description.</param>
    /// <param name="description">The full product description.</param>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="sku">The stock keeping unit.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="discount">The discount percentage.</param>
    /// <param name="brandId">The brand identifier.</param>
    /// <param name="barcode">The product barcode.</param>
    /// <param name="colors">Available colors.</param>
    /// <param name="sizes">Available sizes.</param>
    /// <param name="tags">Product tags.</param>
    /// <param name="stockQuantity">Product stock quantity.</param>
    /// <param name="highlights">Bullet-point product highlights.</param>
    public void Update(
        string name,
        string shortDescription,
        string description,
        CategoryId categoryId,
        string sku,
        Price unitPrice,
        decimal? discount,
        BrandId? brandId,
        string barcode,
        string colors,
        string sizes,
        string tags,
        int stockQuantity,
        string? highlights = null
    )
    {
        var previousEffectivePrice = UnitPrice.Amount * (1 - (Discount ?? 0) / 100m);
        var wasOutOfStock = StockQuantity == 0;

        Name = name;
        ShortDescription = shortDescription;
        Description = description;
        Highlights = highlights;
        CategoryId = categoryId;
        SKU = sku;
        UnitPrice = unitPrice;
        Discount = discount;
        BrandId = brandId;
        Barcode = barcode;
        Colors = colors;
        Sizes = sizes;
        Tags = tags;
        StockQuantity = stockQuantity;

        AddDomainEvent(new Events.ProductUpdatedDomainEvent(this));

        var currentEffectivePrice = UnitPrice.Amount * (1 - (Discount ?? 0) / 100m);
        if (currentEffectivePrice < previousEffectivePrice)
        {
            AddDomainEvent(
                new Events.ProductPriceDroppedDomainEvent(
                    this,
                    previousEffectivePrice,
                    currentEffectivePrice
                )
            );
        }

        if (wasOutOfStock && StockQuantity > 0)
        {
            AddDomainEvent(new Events.ProductBackInStockDomainEvent(this));
        }
    }

    /// <summary>
    /// Reduces the stock quantity by the specified amount.
    /// </summary>
    /// <param name="quantity">The quantity to deduct from stock.</param>
    public void ReduceStock(int quantity) => StockQuantity -= quantity;

    /// <summary>
    /// Increases the stock quantity by the specified amount (e.g., when an order is cancelled or restocked).
    /// </summary>
    /// <param name="quantity">The quantity to restore to stock.</param>
    public void IncreaseStock(int quantity)
    {
        var wasOutOfStock = StockQuantity == 0;
        StockQuantity += quantity;

        if (wasOutOfStock && StockQuantity > 0)
        {
            AddDomainEvent(new Events.ProductBackInStockDomainEvent(this));
        }
    }

    /// <summary>
    /// Adds multiple product images.
    /// </summary>
    /// <param name="productImages">The list of product images to add.</param>
    public void AddProductImages(IReadOnlyList<ProductImage> productImages) =>
        _productImages.AddRange(productImages);

    /// <summary>
    /// Adds a single product image.
    /// </summary>
    /// <param name="productImage">The product image to add.</param>
    public void AddProductImage(ProductImage productImage) => _productImages.Add(productImage);

    /// <summary>
    /// Removes a product image.
    /// </summary>
    /// <param name="productImage">The product image to remove.</param>
    public void RemoveProductImage(ProductImage productImage) =>
        _productImages.Remove(productImage);

    /// <summary>
    /// Increments the favorite count for this product.
    /// </summary>
    public void UpdateFavourite() => Favourites += 1;

    /// <summary>
    /// Incorporates a new review rating into the product's average.
    /// </summary>
    /// <param name="rating"></param>
    public void AddReviewRating(Rating rating) => AverageRating.AddNewRating(rating);

    /// <summary>
    /// Removes a review rating from the product's average.
    /// </summary>
    /// <param name="rating"></param>
    public void RemoveReviewRating(Rating rating) => AverageRating.RemoveRating(rating);

    /// <summary>
    /// Sets the active status of the product.
    /// </summary>
    /// <param name="isActive">Whether the product should be active.</param>
    public void SetIsActive(bool isActive) => IsActive = isActive;

    /// <summary>
    /// Adds a product variant.
    /// </summary>
    /// <param name="variant">The variant to add.</param>
    public void AddVariant(ProductVariant variant) => _productVariants.Add(variant);

    /// <summary>
    /// Updates an existing product variant.
    /// </summary>
    /// <param name="variantId"></param>
    /// <param name="name"></param>
    /// <param name="sku"></param>
    /// <param name="unitPrice"></param>
    /// <param name="stockQuantity"></param>
    /// <param name="isActive"></param>
    public DomainResult<ProductVariant> UpdateVariant(
        ProductVariantId variantId,
        string name,
        string sku,
        Price unitPrice,
        int stockQuantity,
        bool isActive
    )
    {
        var variant = _productVariants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
        {
            return CustomErrors.ProductVariant.VariantNotFound;
        }

        variant.Update(name, sku, unitPrice, stockQuantity, isActive);
        return variant;
    }

    /// <summary>
    /// Removes a product variant.
    /// </summary>
    /// <param name="variantId"></param>
    public DomainResult<bool> RemoveVariant(ProductVariantId variantId)
    {
        var variant = _productVariants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
        {
            return CustomErrors.ProductVariant.VariantNotFound;
        }

        _productVariants.Remove(variant);
        return true;
    }

    private Product(
        ProductId productId,
        string name,
        string shortDescription,
        string description,
        CategoryId categoryId,
        string sku,
        int stockQuantity,
        Price unitPrice,
        decimal? discount,
        BrandId? brandId,
        string barcode,
        string colors,
        string sizes,
        string tags,
        AverageRating averageRating,
        string? highlights = null
    )
        : base(productId)
    {
        Name = name;
        ShortDescription = shortDescription;
        Description = description;
        CategoryId = categoryId;
        SKU = sku;
        StockQuantity = stockQuantity;
        UnitPrice = unitPrice;
        Discount = discount;
        BrandId = brandId;
        Barcode = barcode;
        Colors = colors;
        Sizes = sizes;
        Tags = tags;
        AverageRating = averageRating;
        Highlights = highlights;
    }

    [JsonConstructor]
    private Product() { }
}
