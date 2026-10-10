using System.Globalization;

namespace Shopizy.Application;

/// <summary>
/// Stable cache-key formats so query-side caching and command-side invalidation stay in sync.
/// </summary>
public static class CacheKeys
{
    public const string BrandsAll = "brands-all";
    public const string ProductBrands = "products:brands";

    public static string Brand(Guid brandId) =>
        string.Create(CultureInfo.InvariantCulture, $"brand:{brandId}");

    public const string CategoriesAll = "categories-all";
    public const string CategoriesTree = "categories-tree";

    public static string Category(Guid categoryId) =>
        string.Create(CultureInfo.InvariantCulture, $"category:{categoryId}");

    public static string Category(Shopizy.Domain.Categories.ValueObjects.CategoryId categoryId) =>
        Category(categoryId.Value);

    public static string Product(Guid productId) =>
        string.Create(CultureInfo.InvariantCulture, $"product:{productId}");

    public static string Product(Shopizy.Domain.Products.ValueObjects.ProductId productId) =>
        Product(productId.Value);

    public static string ProductVariants(Guid productId) =>
        string.Create(CultureInfo.InvariantCulture, $"product-variants:{productId}");

    public static string ProductVariants(
        Shopizy.Domain.Products.ValueObjects.ProductId productId
    ) => ProductVariants(productId.Value);

    public static string User(Guid userId) =>
        string.Create(CultureInfo.InvariantCulture, $"user-{userId}");

    public static string User(Shopizy.Domain.Users.ValueObjects.UserId userId) =>
        User(userId.Value);

    public const string AdminDashboardMetrics = "admin:dashboard-metrics";

    public static string AdminTopProducts(int count) =>
        string.Create(CultureInfo.InvariantCulture, $"admin:top-products:{count}");
}
