using Microsoft.AspNetCore.Http;
using Moq;
using Shopizy.Contracts.Admin;
using Shopizy.Contracts.AuditLog;
using Shopizy.Contracts.Authentication;
using Shopizy.Contracts.Cart;
using Shopizy.Contracts.Category;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.GiftCard;
using Shopizy.Contracts.LoyaltyAccount;
using Shopizy.Contracts.Order;
using Shopizy.Contracts.Payment;
using Shopizy.Contracts.Product;
using Shopizy.Contracts.ProductQuestion;
using Shopizy.Contracts.ProductReview;
using Shopizy.Contracts.PromoCode;
using Shopizy.Contracts.User;
using Shopizy.Contracts.Wishlist;
using Shouldly;
using Xunit;

namespace Shopizy.Contracts.UnitTests;

public class ContractDTOsTests
{
    [Fact]
    public void Admin_Contracts_ShouldInitializeProperties()
    {
        var stock = new StockAlertResponse(Guid.NewGuid(), "Product A", 2);
        stock.ShouldNotBeNull();

        var customer = new TopCustomerResponse(Guid.NewGuid(), "Alice", "Smith", 500m);
        customer.ShouldNotBeNull();

        var topProd = new TopProductResponse("Product A", 10, 1000m);
        topProd.ShouldNotBeNull();

        var report = new SalesReportResponse(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            2500m,
            10,
            [topProd]
        );
        report.ShouldNotBeNull();
    }

    [Fact]
    public void Authentication_Contracts_ShouldInitializeProperties()
    {
        var forgotReq = new ForgotPasswordRequest("user@example.com");
        forgotReq.ShouldNotBeNull();

        var forgotRes = new ForgotPasswordResponse("Reset link sent");
        forgotRes.ShouldNotBeNull();

        var refresh = new RefreshTokenRequest("token123");
        refresh.ShouldNotBeNull();

        var reset = new ResetPasswordRequest("token123", "NewPass123!");
        reset.ShouldNotBeNull();
    }

    [Fact]
    public void Cart_Contracts_ShouldInitializeProperties()
    {
        var req = new CreateCartWithFirstProductRequest(Guid.NewGuid(), "Red", "M", 2);
        req.ShouldNotBeNull();
    }

    [Fact]
    public void Payment_Contracts_ShouldInitializeProperties()
    {
        var card = new CardInfo("1234", 12, 2028, "Visa");
        card.ShouldNotBeNull();

        var res = new PaymentResponse("ch_123", "succeeded", 1000, "usd", "pm_123", "cus_123");
        res.ShouldNotBeNull();
    }

    [Fact]
    public void Product_Contracts_ShouldInitializeProperties()
    {
        var mockFile = new Mock<IFormFile>();
        var imgReq = new AddProductImageRequest(mockFile.Object);
        imgReq.ShouldNotBeNull();

        var addVarReq = new AddVariantRequest("Variant 1", "SKU1", 50m, "usd", 10);
        addVarReq.ShouldNotBeNull();

        var detailRevRes = new ProductDetailReviewResponse(
            Guid.NewGuid(),
            "John",
            "Great",
            "Awesome product",
            5m,
            DateTime.UtcNow
        );
        detailRevRes.ShouldNotBeNull();

        var imgRes = new ProductImageResponse(
            Guid.NewGuid(),
            "https://img.com/1.jpg",
            1,
            "main image"
        );
        imgRes.ShouldNotBeNull();

        var varRes = new ProductVariantResponse(
            Guid.NewGuid(),
            "Var 1",
            "SKU1",
            50m,
            "usd",
            10,
            true
        );
        varRes.ShouldNotBeNull();

        var updVarReq = new UpdateVariantRequest("Var 1", "SKU1", 55m, "usd", 15, true);
        updVarReq.ShouldNotBeNull();

        var bulkDeleteReq = new BulkDeleteProductsRequest([Guid.NewGuid(), Guid.NewGuid()]);
        bulkDeleteReq.ShouldNotBeNull();
        bulkDeleteReq.ProductIds.Count.ShouldBe(2);

        var bulkStatusReq = new BulkUpdateProductStatusRequest([Guid.NewGuid()], true);
        bulkStatusReq.ShouldNotBeNull();
        bulkStatusReq.ProductIds.Count.ShouldBe(1);
        bulkStatusReq.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void ProductQuestion_Contracts_ShouldInitializeProperties()
    {
        var ansReq = new AnswerQuestionRequest("This is the answer");
        ansReq.ShouldNotBeNull();

        var askReq = new AskQuestionRequest("What is the warranty?");
        askReq.ShouldNotBeNull();

        var qRes = new ProductQuestionResponse(
            Guid.NewGuid(),
            "Warranty?",
            true,
            "1 Year",
            DateTime.UtcNow
        );
        qRes.ShouldNotBeNull();
    }

    [Fact]
    public void ProductReview_Contracts_ShouldInitializeProperties()
    {
        var revRes = new ProductReviewResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alice",
            5m,
            "Great product!",
            DateTime.UtcNow
        );
        revRes.ShouldNotBeNull();
    }

    [Fact]
    public void PromoCode_Contracts_ShouldInitializeProperties()
    {
        var createReq = new CreatePromoCodeRequest("PROMO10", "10% Off", 10m, true, true);
        createReq.ShouldNotBeNull();

        var pRes = new PromoCodeResponse(
            Guid.NewGuid(),
            "PROMO10",
            "10% Off",
            10m,
            true,
            true,
            5,
            DateTime.UtcNow
        );
        pRes.ShouldNotBeNull();

        var updReq = new UpdatePromoCodeRequest("PROMO20", "20% Off", 20m, true, false);
        updReq.ShouldNotBeNull();
    }

    [Fact]
    public void User_Contracts_ShouldInitializeProperties()
    {
        var addAddrReq = new AddAddressRequest(
            "Main St",
            "City",
            "State",
            "Country",
            "12345",
            true
        );
        addAddrReq.ShouldNotBeNull();

        var addUserAddrReq = new AddUserAddressRequest(
            "Main St",
            "City",
            "State",
            "Country",
            "12345",
            true
        );
        addUserAddrReq.ShouldNotBeNull();

        var updUserAddrReq = new UpdateUserAddressRequest(
            "Main St",
            "City",
            "State",
            "Country",
            "12345"
        );
        updUserAddrReq.ShouldNotBeNull();

        var addrRes = new UserAddressResponse(
            Guid.NewGuid(),
            "Main St",
            "City",
            "State",
            "Country",
            "12345",
            true,
            DateTime.UtcNow
        );
        addrRes.ShouldNotBeNull();
    }

    [Fact]
    public void Wishlist_Contracts_ShouldInitializeProperties()
    {
        var updSettingsReq = new UpdateWishlistSettingsRequest("Favorites", true);
        updSettingsReq.ShouldNotBeNull();
        updSettingsReq.Name.ShouldBe("Favorites");
        updSettingsReq.IsPublic.ShouldBeTrue();

        var createWishlistReq = new CreateWishlistRequest("My List", true);
        createWishlistReq.Name.ShouldBe("My List");
        createWishlistReq.IsPublic.ShouldBeTrue();

        var updateWishlistReq = new UpdateWishlistRequest(Guid.NewGuid(), "Add");
        updateWishlistReq.Action.ShouldBe("Add");

        var itemRes = new WishlistItemResponse(Guid.NewGuid(), Guid.NewGuid());
        itemRes.ProductId.ShouldNotBe(Guid.Empty);

        var listRes = new WishlistResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "My List",
            true,
            DateTime.UtcNow,
            null,
            [itemRes]
        );
        listRes.WishlistItems.Count.ShouldBe(1);
    }

    [Fact]
    public void Category_Contracts_ShouldInitializeProperties()
    {
        var createCatReq = new CreateCategoryRequest("Electronics", null);
        createCatReq.Name.ShouldBe("Electronics");

        var updCatReq = new UpdateCategoryRequest("Gadgets", null);
        updCatReq.Name.ShouldBe("Gadgets");

        var catRes = new CategoryResponse(Guid.NewGuid(), "Electronics", null);
        catRes.Name.ShouldBe("Electronics");

        var catTree = new CategoryTreeResponse(Guid.NewGuid(), "Electronics", null, []);
        catTree.Children!.ShouldBeEmpty();
    }

    [Fact]
    public void GiftCardAndLoyalty_Contracts_ShouldInitializeProperties()
    {
        var createGc = new CreateGiftCardRequest("GIFT100", 100m, null);
        createGc.Code.ShouldBe("GIFT100");

        var gcRes = new GiftCardResponse(
            Guid.NewGuid(),
            "GIFT100",
            100m,
            100m,
            true,
            null,
            DateTime.UtcNow
        );
        gcRes.Code.ShouldBe("GIFT100");

        var redeemGc = new RedeemGiftCardRequest("GIFT100");
        redeemGc.Code.ShouldBe("GIFT100");

        var earnPts = new EarnPointsRequest(100, "Purchase");
        earnPts.Points.ShouldBe(100);

        var redeemPts = new RedeemPointsRequest(50, "Order #1");
        redeemPts.Points.ShouldBe(50);

        var txRes = new LoyaltyTransactionResponse(
            Guid.NewGuid(),
            100,
            "Earn",
            "Purchase",
            DateTime.UtcNow
        );
        txRes.Points.ShouldBe(100);

        var loyaltyRes = new LoyaltyAccountResponse(Guid.NewGuid(), 500, [txRes]);
        loyaltyRes.TotalPoints.ShouldBe(500);
    }

    [Fact]
    public void Product_Contracts_ShouldInitializeProperties_Expanded()
    {
        var brandRes = new BrandResponse(Guid.NewGuid(), "Nike", "logo.png", "USA");
        brandRes.Name.ShouldBe("Nike");

        var createBrand = new CreateBrandRequest("Nike", "logo.png", "USA");
        createBrand.Name.ShouldBe("Nike");

        var updBrand = new UpdateBrandRequest("Nike Inc", "logo.png", "USA");
        updBrand.Name.ShouldBe("Nike Inc");

        var avgRating = new AverageRating(4.5m, 10);
        var prodRes = new ProductResponse(
            Guid.NewGuid(),
            "Shoes",
            "Short",
            "Desc",
            Guid.NewGuid(),
            "$100",
            0m,
            Guid.NewGuid(),
            "M",
            "Red",
            "Tag",
            "123",
            10,
            avgRating,
            []
        );
        prodRes.Name.ShouldBe("Shoes");

        var prodDetail = new ProductDetailResponse(
            Guid.NewGuid(),
            "Shoes",
            "Short",
            "Long",
            Guid.NewGuid(),
            "$100",
            0m,
            "SKU1",
            Guid.NewGuid(),
            "M",
            "Red",
            "Tag",
            "123",
            10,
            avgRating,
            5,
            [],
            [],
            []
        );
        prodDetail.Name.ShouldBe("Shoes");

        var criteria = new ProductsCriteria(
            null,
            "Shoes",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1,
            10
        );
        criteria.Name.ShouldBe("Shoes");

        var pagedRes = new ProductsPagedResponse([prodRes], 1, 1, 1);
        pagedRes.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void UserAndAuth_Contracts_ShouldInitializeProperties()
    {
        var authRes = new AuthResponse(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john@example.com",
            "Customer",
            "token",
            "refresh",
            DateTime.UtcNow
        );
        authRes.Token.ShouldBe("token");

        var loginReq = new LoginRequest("john@example.com", "Password123!");
        loginReq.Email.ShouldBe("john@example.com");

        var regReq = new RegisterRequest("John", "Doe", "john@example.com", "Password123!");
        regReq.FirstName.ShouldBe("John");

        var updAddr = new UpdateAddressRequest
        {
            Street = "Main St",
            City = "City",
            State = "State",
            Country = "Country",
            ZipCode = "12345",
        };
        var updUser = new UpdateUserRequest("John", "Doe", "1234567890", updAddr);
        updUser.FirstName.ShouldBe("John");

        var updPass = new UpdatePasswordRequest
        {
            OldPassword = "Old123!",
            NewPassword = "New123!",
        };
        updPass.OldPassword.ShouldBe("Old123!");

        var userDetails = new UserDetails(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john@example.com",
            "img.png",
            "1234567890",
            null,
            5,
            2,
            0,
            0,
            DateTime.UtcNow,
            null
        );
        userDetails.TotalOrders.ShouldBe(5);
    }
}

public class TopCustomerResponseTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldHoldCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var sut = new TopCustomerResponse(userId, "Jane", "Doe", 1250.75m);

        sut.UserId.ShouldBe(userId);
        sut.FirstName.ShouldBe("Jane");
        sut.LastName.ShouldBe("Doe");
        sut.TotalSpend.ShouldBe(1250.75m);
    }

    [Fact]
    public void TwoInstances_WithSameValues_ShouldBeEqual()
    {
        var userId = Guid.NewGuid();
        var a = new TopCustomerResponse(userId, "Jane", "Doe", 500m);
        var b = new TopCustomerResponse(userId, "Jane", "Doe", 500m);

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void TwoInstances_WithDifferentUserId_ShouldNotBeEqual()
    {
        var a = new TopCustomerResponse(Guid.NewGuid(), "Jane", "Doe", 500m);
        var b = new TopCustomerResponse(Guid.NewGuid(), "Jane", "Doe", 500m);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void TwoInstances_WithDifferentSpend_ShouldNotBeEqual()
    {
        var userId = Guid.NewGuid();
        var a = new TopCustomerResponse(userId, "Jane", "Doe", 100m);
        var b = new TopCustomerResponse(userId, "Jane", "Doe", 999m);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void WithExpression_ShouldProduceUpdatedCopy()
    {
        var original = new TopCustomerResponse(Guid.NewGuid(), "Jane", "Doe", 500m);
        var updated = original with { TotalSpend = 750m, FirstName = "Alice" };

        // Original must be unchanged
        original.FirstName.ShouldBe("Jane");
        original.TotalSpend.ShouldBe(500m);

        // Copy must reflect changes
        updated.FirstName.ShouldBe("Alice");
        updated.TotalSpend.ShouldBe(750m);
        updated.LastName.ShouldBe(original.LastName);
        updated.UserId.ShouldBe(original.UserId);
    }

    [Fact]
    public void TotalSpend_WhenZero_ShouldBeAllowed()
    {
        var sut = new TopCustomerResponse(Guid.NewGuid(), "New", "User", 0m);
        sut.TotalSpend.ShouldBe(0m);
    }

    [Fact]
    public void ToString_ShouldContainPropertyValues()
    {
        var userId = Guid.NewGuid();
        var sut = new TopCustomerResponse(userId, "Jane", "Doe", 500m);
        var str = sut.ToString();

        str.ShouldContain("Jane");
        str.ShouldContain("Doe");
        str.ShouldContain("500");
    }
}

public class SalesReportResponseTests
{
    private static TopProductResponse MakeTopProduct(
        string name = "Widget",
        int sold = 5,
        decimal revenue = 250m
    ) => new(name, sold, revenue);

    [Fact]
    public void Create_WithValidArguments_ShouldHoldCorrectProperties()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);
        var products = new List<TopProductResponse> { MakeTopProduct("Gadget", 10, 500m) };

        var sut = new SalesReportResponse(start, end, 2500m, 42, products);

        sut.StartDate.ShouldBe(start);
        sut.EndDate.ShouldBe(end);
        sut.TotalRevenue.ShouldBe(2500m);
        sut.TotalOrders.ShouldBe(42);
        sut.TopProducts.ShouldNotBeNull();
        sut.TopProducts.Count.ShouldBe(1);
        sut.TopProducts[0].Name.ShouldBe("Gadget");
    }

    [Fact]
    public void TwoInstances_WithSameValues_ShouldBeEqual()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(30);
        var prods = (IReadOnlyList<TopProductResponse>)[MakeTopProduct()];

        var a = new SalesReportResponse(start, end, 1000m, 5, prods);
        var b = new SalesReportResponse(start, end, 1000m, 5, prods);

        a.ShouldBe(b);
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void TwoInstances_WithDifferentRevenue_ShouldNotBeEqual()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(30);
        var prods = (IReadOnlyList<TopProductResponse>)[];

        var a = new SalesReportResponse(start, end, 1000m, 5, prods);
        var b = new SalesReportResponse(start, end, 9999m, 5, prods);

        a.ShouldNotBe(b);
    }

    [Fact]
    public void WithExpression_ShouldProduceUpdatedCopy()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(30);
        var original = new SalesReportResponse(start, end, 1000m, 5, []);

        var updated = original with { TotalRevenue = 5000m, TotalOrders = 20 };

        // Original must be unchanged
        original.TotalRevenue.ShouldBe(1000m);
        original.TotalOrders.ShouldBe(5);

        // Copy must reflect changes
        updated.TotalRevenue.ShouldBe(5000m);
        updated.TotalOrders.ShouldBe(20);
        updated.StartDate.ShouldBe(original.StartDate);
        updated.EndDate.ShouldBe(original.EndDate);
    }

    [Fact]
    public void TopProducts_WhenEmpty_ShouldReturnEmptyList()
    {
        var sut = new SalesReportResponse(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, 0m, 0, []);

        sut.TopProducts.ShouldNotBeNull();
        sut.TopProducts.Count.ShouldBe(0);
    }

    [Fact]
    public void TopProducts_WhenMultiple_ShouldPreserveOrder()
    {
        var p1 = MakeTopProduct("First", 10, 500m);
        var p2 = MakeTopProduct("Second", 5, 250m);
        var p3 = MakeTopProduct("Third", 1, 50m);

        var sut = new SalesReportResponse(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            800m,
            16,
            [p1, p2, p3]
        );

        sut.TopProducts[0].Name.ShouldBe("First");
        sut.TopProducts[1].Name.ShouldBe("Second");
        sut.TopProducts[2].Name.ShouldBe("Third");
    }

    [Fact]
    public void TotalRevenue_WhenZero_ShouldBeAllowed()
    {
        var sut = new SalesReportResponse(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, 0m, 0, []);

        sut.TotalRevenue.ShouldBe(0m);
        sut.TotalOrders.ShouldBe(0);
    }

    [Fact]
    public void ToString_ShouldContainPropertyValues()
    {
        var sut = new SalesReportResponse(
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            999m,
            7,
            []
        );

        var str = sut.ToString();
        str.ShouldContain("999");
        str.ShouldContain("7");
    }
}
