using Shouldly;

namespace Shopizy.Application.UnitTests.Common.Caching;

public class CacheKeysTests
{
    [Fact]
    public void Product_ShouldReturnFormattedKey()
    {
        var productId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        CacheKeys.Product(productId).ShouldBe("product:11111111-2222-3333-4444-555555555555");
    }

    [Fact]
    public void ProductVariants_ShouldReturnFormattedKey()
    {
        var productId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        CacheKeys
            .ProductVariants(productId)
            .ShouldBe("product-variants:11111111-2222-3333-4444-555555555555");
    }

    [Fact]
    public void BrandKeys_ShouldReturnExpectedKeys()
    {
        var brandId = Guid.Parse("22222222-3333-4444-5555-666666666666");
        CacheKeys.BrandsAll.ShouldBe("brands-all");
        CacheKeys.ProductBrands.ShouldBe("products:brands");
        CacheKeys.Brand(brandId).ShouldBe("brand:22222222-3333-4444-5555-666666666666");
    }

    [Fact]
    public void CategoryKeys_ShouldReturnExpectedKeys()
    {
        var categoryId = Guid.Parse("33333333-4444-5555-6666-777777777777");
        CacheKeys.CategoriesAll.ShouldBe("categories-all");
        CacheKeys.CategoriesTree.ShouldBe("categories-tree");
        CacheKeys.Category(categoryId).ShouldBe("category:33333333-4444-5555-6666-777777777777");
    }

    [Fact]
    public void User_ShouldReturnFormattedKey()
    {
        var userId = Guid.Parse("44444444-5555-6666-7777-888888888888");
        CacheKeys.User(userId).ShouldBe("user-44444444-5555-6666-7777-888888888888");
    }

    [Fact]
    public void AdminKeys_ShouldReturnExpectedKeys()
    {
        CacheKeys.AdminDashboardMetrics.ShouldBe("admin:dashboard-metrics");
        CacheKeys.AdminTopProducts(10).ShouldBe("admin:top-products:10");
    }
}
