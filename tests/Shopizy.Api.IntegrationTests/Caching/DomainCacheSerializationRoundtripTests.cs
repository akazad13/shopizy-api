using System.Text.Json;
using ErrorOr;
using Mapster;
using MapsterMapper;
using Shopizy.Api.Common.Mapping;
using Shopizy.Contracts.Cart;
using Shopizy.Contracts.Category;
using Shopizy.Contracts.Product;
using Shopizy.Contracts.ProductReview;
using Shopizy.Contracts.Wishlist;
using Shopizy.Domain.Brands;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.Entities;
using Shopizy.Domain.Categories;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.ProductReviews;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.Entities;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.Domain.Wishlists;
using Shopizy.Infrastructure.Common.Caching;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Caching;

public class DomainCacheSerializationRoundtripTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IMapper _mapper;

    public DomainCacheSerializationRoundtripTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            Converters = { new ErrorOrConverterFactory() },
            TypeInfoResolver = new PrivateSetterContractResolver(),
        };

        var config = new TypeAdapterConfig();
        new ProductMappingConfig().Register(config);
        new CategoryMappingConfig().Register(config);
        new CartMappingConfig().Register(config);
        new WishlistMappingConfig().Register(config);
        new ProductReviewMappingConfig().Register(config);
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Product_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithoutDataLoss()
    {
        // Arrange
        var categoryId = CategoryId.CreateUnique();
        var brandId = Shopizy.Domain.Brands.ValueObjects.BrandId.CreateUnique();
        var product = Product.Create(
            "Test Product",
            "Short description",
            "Detailed product description",
            categoryId,
            "SKU-12345",
            50,
            Price.CreateNew(99.99m, Currency.usd),
            10m,
            brandId,
            "BARCODE-99",
            "Red,Blue",
            "S,M,L",
            "fashion,sale",
            "Hand cut and sewn locally\nPre-washed & pre-shrunk"
        );

        product.AddProductImage(ProductImage.Create("https://example.com/image.jpg", 1, "pub_123"));
        product.AddVariant(
            ProductVariant.Create(
                "Red / S",
                "SKU-12345-R-S",
                Price.CreateNew(99.99m, Currency.usd),
                20
            )
        );

        ErrorOr<Product> originalResult = product;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<Product>>(json, _jsonOptions);

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedProduct = deserializedResult.Value;
        deserializedProduct.ShouldNotBeNull();
        deserializedProduct.Id.ShouldNotBeNull();
        deserializedProduct.Id.Value.ShouldBe(product.Id.Value);
        deserializedProduct.CategoryId.ShouldNotBeNull();
        deserializedProduct.CategoryId.Value.ShouldBe(categoryId.Value);
        deserializedProduct.BrandId.ShouldNotBeNull();
        deserializedProduct.BrandId!.Value.ShouldBe(brandId.Value);
        deserializedProduct.UnitPrice.ShouldNotBeNull();
        deserializedProduct.UnitPrice.Amount.ShouldBe(99.99m);
        deserializedProduct.AverageRating.ShouldNotBeNull();
        deserializedProduct.Highlights.ShouldBe(
            "Hand cut and sewn locally\nPre-washed & pre-shrunk"
        );
        deserializedProduct.ProductImages.Count.ShouldBe(1);
        deserializedProduct.ProductVariants.Count.ShouldBe(1);

        // Act: Mapster mapping to response DTOs
        var productResponse = _mapper.Map<ProductResponse>(deserializedProduct);
        productResponse.ShouldNotBeNull();
        productResponse.ProductId.ShouldBe(product.Id.Value);
        productResponse.CategoryId.ShouldBe(categoryId.Value);
        productResponse.BrandId.ShouldBe(brandId.Value);

        var productDetailResponse = _mapper.Map<ProductDetailResponse>(deserializedProduct);
        productDetailResponse.ShouldNotBeNull();
        productDetailResponse.ProductId.ShouldBe(product.Id.Value);
        productDetailResponse.Highlights.ShouldBe(
            "Hand cut and sewn locally\nPre-washed & pre-shrunk"
        );
    }

    [Fact]
    public void Category_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithNonNullId()
    {
        // Arrange
        var category = Category.Create("Electronics", null);
        ErrorOr<Category> originalResult = category;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<Category>>(json, _jsonOptions);

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedCategory = deserializedResult.Value;
        deserializedCategory.ShouldNotBeNull();
        deserializedCategory.Id.ShouldNotBeNull();
        deserializedCategory.Id.Value.ShouldBe(category.Id.Value);
        deserializedCategory.Name.ShouldBe("Electronics");

        var response = _mapper.Map<CategoryResponse>(deserializedCategory);
        response.ShouldNotBeNull();
        response.Id.ShouldBe(category.Id.Value);
    }

    [Fact]
    public void Brand_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithNonNullId()
    {
        // Arrange
        var brand = Brand.Create("Nike", "USA", "https://example.com/nike.png");
        ErrorOr<Brand> originalResult = brand;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<Brand>>(json, _jsonOptions);

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedBrand = deserializedResult.Value;
        deserializedBrand.ShouldNotBeNull();
        deserializedBrand.Id.ShouldNotBeNull();
        deserializedBrand.Id.Value.ShouldBe(brand.Id.Value);
        deserializedBrand.Name.ShouldBe("Nike");
    }

    [Fact]
    public void Cart_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithItemsAndIds()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var productId = ProductId.CreateUnique();
        var cart = Cart.Create(userId);
        cart.AddLineItem(CartItem.Create(productId, "Blue", "M", 2));
        ErrorOr<Cart> originalResult = cart;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<Cart>>(json, _jsonOptions);

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedCart = deserializedResult.Value;
        deserializedCart.ShouldNotBeNull();
        deserializedCart.Id.ShouldNotBeNull();
        deserializedCart.Id.Value.ShouldBe(cart.Id.Value);
        deserializedCart.UserId.ShouldNotBeNull();
        deserializedCart.UserId.Value.ShouldBe(userId.Value);
        deserializedCart.CartItems.Count.ShouldBe(1);
        deserializedCart.CartItems[0].ProductId.ShouldNotBeNull();
        deserializedCart.CartItems[0].ProductId.Value.ShouldBe(productId.Value);

        var cartResponse = _mapper.Map<CartResponse>(deserializedCart);
        cartResponse.ShouldNotBeNull();
        cartResponse.CartId.ShouldBe(cart.Id.Value);
        cartResponse.UserId.ShouldBe(userId.Value);
    }

    [Fact]
    public void Wishlist_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithItemsAndIds()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var productId = ProductId.CreateUnique();
        var wishlist = Wishlist.Create(userId, "My Favorites", isPublic: true);
        wishlist.AddItem(productId);
        ErrorOr<Wishlist> originalResult = wishlist;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<Wishlist>>(json, _jsonOptions);

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedWishlist = deserializedResult.Value;
        deserializedWishlist.ShouldNotBeNull();
        deserializedWishlist.Id.ShouldNotBeNull();
        deserializedWishlist.Id.Value.ShouldBe(wishlist.Id.Value);
        deserializedWishlist.UserId.ShouldNotBeNull();
        deserializedWishlist.UserId.Value.ShouldBe(userId.Value);
        deserializedWishlist.WishlistItems.Count.ShouldBe(1);

        var response = _mapper.Map<WishlistResponse>(deserializedWishlist);
        response.ShouldNotBeNull();
        response.WishlistId.ShouldBe(wishlist.Id.Value);
    }

    [Fact]
    public void ProductReview_ErrorOr_CacheRoundtrip_ShouldSerializeAndDeserializeWithRating()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var productId = ProductId.CreateUnique();
        var rating = Rating.CreateNew(5m);
        var review = ProductReview.Create(
            userId,
            productId,
            rating,
            "Amazing quality product!",
            isVerifiedPurchase: true,
            headline: "Loved it!",
            imageUrls: new[] { "https://example.com/photo1.jpg" }
        );
        ErrorOr<ProductReview> originalResult = review;

        // Act
        var json = JsonSerializer.Serialize(originalResult, _jsonOptions);
        var deserializedResult = JsonSerializer.Deserialize<ErrorOr<ProductReview>>(
            json,
            _jsonOptions
        );

        // Assert
        deserializedResult.IsError.ShouldBeFalse();
        var deserializedReview = deserializedResult.Value;
        deserializedReview.ShouldNotBeNull();
        deserializedReview.Id.ShouldNotBeNull();
        deserializedReview.Id.Value.ShouldBe(review.Id.Value);
        deserializedReview.Rating.ShouldNotBeNull();
        deserializedReview.Rating.Value.ShouldBe(5m);
        deserializedReview.ImageUrls.Count.ShouldBe(1);

        var response = _mapper.Map<ProductReviewResponse>(deserializedReview);
        response.ShouldNotBeNull();
        response.ReviewId.ShouldBe(review.Id.Value);
        response.Rating.ShouldBe(5m);
    }
}
