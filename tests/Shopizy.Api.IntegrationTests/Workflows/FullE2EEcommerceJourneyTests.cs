using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Shopizy.Contracts.Admin;
using Shopizy.Contracts.Cart;
using Shopizy.Contracts.Category;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.LoyaltyAccount;
using Shopizy.Contracts.Order;
using Shopizy.Contracts.Payment;
using Shopizy.Contracts.Product;
using Shopizy.Contracts.ProductQuestion;
using Shopizy.Contracts.ProductReview;
using Shopizy.Contracts.PromoCode;
using Shopizy.Contracts.Returns;
using Shopizy.Contracts.User;
using Shopizy.Contracts.Wishlist;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Workflows;

public class FullE2EEcommerceJourneyTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task FullRetailJourney_FromCatalogSetupToDeliveryAndReturn_SucceedsEndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;

        // =========================================================================
        // STEP 1: Admin Sets Up Catalog (Brand, Category, Product, Variant)
        // =========================================================================
        var (adminToken, adminUserId) = await AuthenticateAsAdminAsync();

        // 1a. Create Brand
        var brandRequest = new CreateBrandRequest(
            $"Sony-{Guid.NewGuid().ToString()[..4]}",
            "https://example.com/logo.png",
            "Japan"
        );
        var brandResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/brands",
            brandRequest,
            ct
        );
        brandResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var brand = await brandResponse.Content.ReadFromJsonAsync<BrandResponse>(ct);
        brand.ShouldNotBeNull();

        // 1b. Create Category
        var categoryRequest = new CreateCategoryRequest(
            $"Audio Gear {Guid.NewGuid().ToString()[..4]}",
            null
        );
        var categoryResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            categoryRequest,
            ct
        );
        categoryResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>(ct);
        category.ShouldNotBeNull();

        // 1c. Create Product
        var productSku = $"WH1000-{Guid.NewGuid().ToString()[..6]}";
        var productRequest = new CreateProductRequest(
            Name: "Wireless Noise Canceling Headphones",
            ShortDescription: "Flagship wireless ANC headphones",
            Description: "Industry-leading noise cancellation with exceptional sound clarity.",
            CategoryId: category.Id,
            UnitPrice: 349.99m,
            Currency: 1, // USD
            Discount: 0m,
            Sku: productSku,
            BrandId: brand.Id,
            Colors: "Black,Silver",
            Sizes: "Standard",
            Tags: "audio,bluetooth,anc,headphones",
            Barcode: "123456789012",
            StockQuantity: 100,
            SpecificationIds: null,
            Highlights: "30h battery life\nMultipoint pairing\nTouch controls"
        );
        var productResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/products",
            productRequest,
            ct
        );
        productResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>(ct);
        product.ShouldNotBeNull();

        // 1d. Add Variant to Product
        var variantRequest = new AddVariantRequest(
            "Midnight Blue Edition",
            $"{productSku}-BLU",
            369.99m,
            "USD",
            25
        );
        var variantResponse = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/admin/products/{product.ProductId}/variants",
            variantRequest,
            ct
        );
        variantResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 1e. Admin creates a promo code (20% off)
        var promoCodeStr = $"PROMO-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
        var promoRequest = new CreatePromoCodeRequest(
            promoCodeStr,
            "20% off audio gear",
            20.00m,
            true,
            true
        );
        var promoResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/promo-codes",
            promoRequest,
            ct
        );
        promoResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        ClearAuthToken();

        // =========================================================================
        // STEP 2: Customer Registers and Sets Up Profile & Addresses
        // =========================================================================
        var customerEmail = $"shopper-{Guid.NewGuid().ToString()[..8]}@example.com";
        var (customerToken, customerUserId) = await AuthenticateAsNewUserAsync(
            "Alice",
            "Shopper",
            customerEmail,
            "StrongPass@1234!"
        );

        // 2a. Update Notification Preferences
        var notifReq = new UpdateNotificationPreferencesRequest(
            EmailEnabled: true,
            OrderUpdates: true,
            Promotions: false,
            PriceAlerts: true,
            RestockAlerts: true
        );
        var notifResp = await HttpClient.PutAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/notification-preferences",
            notifReq,
            ct
        );
        notifResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 2b. Add Shipping Address
        var addressReq = new AddUserAddressRequest(
            "742 Evergreen Terrace",
            "Springfield",
            "OR",
            "United States",
            "97477",
            true
        );
        var addressResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/addresses",
            addressReq,
            ct
        );
        addressResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var addedAddress = await addressResp.Content.ReadFromJsonAsync<UserAddressResponse>(ct);
        addedAddress.ShouldNotBeNull();
        addedAddress.IsDefault.ShouldBeTrue();

        // 2c. Add Secondary Address & Set Default
        var secondaryAddressReq = new AddUserAddressRequest(
            "100 Elm Street",
            "Portland",
            "OR",
            "United States",
            "97201",
            false
        );
        var secAddressResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/addresses",
            secondaryAddressReq,
            ct
        );
        secAddressResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var secondaryAddress = await secAddressResp.Content.ReadFromJsonAsync<UserAddressResponse>(
            ct
        );
        secondaryAddress.ShouldNotBeNull();

        // Set the secondary address as default
        var setDefaultResp = await HttpClient.PatchAsync(
            $"/api/v1.0/users/{customerUserId}/addresses/{secondaryAddress.AddressId}/set-default",
            null,
            ct
        );
        setDefaultResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // =========================================================================
        // STEP 3: Customer Browses & Interacts with Product (Faceted Search, Q&A)
        // =========================================================================
        // 3a. Search Products by keyword
        var searchResp = await HttpClient.GetAsync(
            "/api/v1.0/products?pageNumber=1&pageSize=10&name=Wireless",
            ct
        );
        searchResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 3b. Customer asks a product question
        var askQuestionReq = new AskQuestionRequest(
            "Does this model support multipoint Bluetooth pairing?"
        );
        var askResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/products/{product.ProductId}/questions",
            askQuestionReq,
            ct
        );
        askResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var question = await askResp.Content.ReadFromJsonAsync<ProductQuestionResponse>(ct);
        question.ShouldNotBeNull();

        // 3c. Admin answers the question
        SetAuthToken(adminToken);
        var answerReq = new AnswerQuestionRequest(
            "Yes, it connects to two Bluetooth devices simultaneously."
        );
        var answerResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/admin/questions/{question.QuestionId}/answer",
            answerReq,
            ct
        );
        answerResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 3d. Admin grants loyalty points to the customer
        var earnPointsReq = new EarnPointsRequest(200, "Welcome Loyalty Bonus");
        var earnResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/loyalty/earn",
            earnPointsReq,
            ct
        );
        earnResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Switch back to customer
        SetAuthToken(customerToken);

        // Verify customer sees answered question
        var listQuestionsResp = await HttpClient.GetAsync(
            $"/api/v1.0/products/{product.ProductId}/questions?pageNumber=1&pageSize=10",
            ct
        );
        listQuestionsResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var questions = await listQuestionsResp.Content.ReadFromJsonAsync<
            List<ProductQuestionResponse>
        >(ct);
        questions.ShouldNotBeNull();
        questions.ShouldContain(q =>
            q.QuestionId == question.QuestionId && q.IsAnswered && q.Answer != null
        );

        // =========================================================================
        // STEP 4: Wishlist Management & Cart Building
        // =========================================================================
        // 4a. Customer creates a wishlist
        var wishlistReq = new CreateWishlistRequest("Holiday Audio Wishlist", true);
        var wishlistResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/wishlist",
            wishlistReq,
            ct
        );
        wishlistResp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var wishlist = await wishlistResp.Content.ReadFromJsonAsync<WishlistResponse>(ct);
        wishlist.ShouldNotBeNull();

        // 4b. Add Product to Wishlist
        var updateWishlistReq = new UpdateWishlistRequest(product.ProductId, "Add");
        var addWishlistResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/wishlist",
            updateWishlistReq,
            ct
        );
        addWishlistResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 4c. Add Product to Cart
        var addToCartReq = new AddProductToCartRequest(product.ProductId, "Black", "Standard", 1);
        var addToCartResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/cart/items",
            addToCartReq,
            ct
        );
        addToCartResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 4d. Verify Cart
        var cartResp = await HttpClient.GetAsync($"/api/v1.0/users/{customerUserId}/cart", ct);
        cartResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cart = await cartResp.Content.ReadFromJsonAsync<CartResponse>(ct);
        cart.ShouldNotBeNull();
        cart.CartItems.ShouldContain(i => i.ProductId == product.ProductId);

        // =========================================================================
        // STEP 5: Promo Validation & Checkout Placement
        // =========================================================================
        // 5a. Validate promo code
        var validatePromoResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/orders/validate-promo",
            promoCodeStr,
            ct
        );
        validatePromoResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var promoValidation = await validatePromoResp.Content.ReadFromJsonAsync<PromoCodeResponse>(
            ct
        );
        promoValidation.ShouldNotBeNull();
        promoValidation.Discount.ShouldBe(20.00m);

        // 5b. Place Order with Promo Code and Loyalty Points
        var orderItems = new List<OrderItemRequest>
        {
            new(product.ProductId, "Black", "Standard", 1),
        };
        var checkoutAddress = new Address(
            "100 Elm Street",
            "Portland",
            "OR",
            "United States",
            "97201"
        );
        var checkoutRequest = new CreateOrderRequest(
            PromoCode: promoCodeStr,
            GiftCardCode: null,
            DeliveryMethod: 1,
            DeliveryCharge: new Price(9.99m, "USD"),
            OrderItems: orderItems,
            ShippingAddress: checkoutAddress,
            LoyaltyPointsToRedeem: 50
        );

        var checkoutResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/orders/checkout",
            checkoutRequest,
            ct
        );
        checkoutResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var order = await checkoutResp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        order.ShouldNotBeNull();
        order.OrderStatus.ShouldBe("Pending");
        order.OrderItems.ShouldNotBeEmpty();
        var orderItemId = order.OrderItems[0].OrderItemId;

        // 5c. Verify Cart is Cleared after checkout
        var cartAfterCheckout = await HttpClient.GetAsync(
            $"/api/v1.0/users/{customerUserId}/cart",
            ct
        );
        var cartAfter = await cartAfterCheckout.Content.ReadFromJsonAsync<CartResponse>(ct);
        cartAfter!.CartItems.ShouldBeEmpty();

        // =========================================================================
        // STEP 6: Payment Processing
        // =========================================================================
        var paymentRequest = new CardNotPresentSaleRequest(
            OrderId: order.OrderId,
            Amount: order.OrderItems.Sum(i => i.UnitPrice.Amount),
            Currency: "USD",
            PaymentMethod: "card",
            PaymentMethodId: "pm_card_visa",
            CardInfo: null
        );
        var paymentResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/payments",
            paymentRequest,
            ct
        );
        paymentResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 6b. Verify Order Status transitioned to Processing
        var orderAfterPaymentResp = await HttpClient.GetAsync(
            $"/api/v1.0/users/{customerUserId}/orders/{order.OrderId}",
            ct
        );
        orderAfterPaymentResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var orderAfterPayment =
            await orderAfterPaymentResp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        orderAfterPayment.ShouldNotBeNull();
        orderAfterPayment.OrderStatus.ShouldBe("Processing");
        orderAfterPayment.PaymentStatus.ShouldBe("Payed");

        // =========================================================================
        // STEP 7: Admin Order Fulfillment (Shipment & Delivery)
        // =========================================================================
        SetAuthToken(adminToken);

        // 7a. Admin Adds Shipment to Order
        var shipmentReq = new CreateShipmentRequest(
            "FedEx",
            "TRK-FE-889977",
            DateTime.UtcNow.AddDays(3)
        );
        var addShipmentResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/admin/orders/{order.OrderId}/shipment",
            shipmentReq,
            ct
        );
        addShipmentResp.StatusCode.ShouldBe(HttpStatusCode.Created);

        // 7b. Admin updates order status to Shipped (3)
        var updateStatusResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/admin/orders/{order.OrderId}/status",
            (int)Domain.Orders.Enums.OrderStatus.Shipping,
            ct
        );
        updateStatusResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 7c. Admin marks order Delivered (4)
        var deliverStatusResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/admin/orders/{order.OrderId}/status",
            (int)Domain.Orders.Enums.OrderStatus.Delivered,
            ct
        );
        deliverStatusResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Switch back to customer
        SetAuthToken(customerToken);

        // Customer verifies order tracking
        var trackingResp = await HttpClient.GetAsync(
            $"/api/v1.0/orders/{order.OrderId}/tracking",
            ct
        );
        trackingResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // =========================================================================
        // STEP 8: Customer Reviews & Return Request
        // =========================================================================
        // 8a. Customer submits product review
        var reviewReq = new CreateProductReviewRequest(
            5,
            "Exceptional noise cancellation and crisp bass!",
            "Best headphones!",
            null
        );
        var reviewResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/products/{product.ProductId}/reviews",
            reviewReq,
            ct
        );
        reviewResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var review = await reviewResp.Content.ReadFromJsonAsync<ProductReviewResponse>(ct);
        review.ShouldNotBeNull();
        review.Rating.ShouldBe(5);

        // 8b. Anonymous upvotes review
        ClearAuthToken();
        var upvoteResp = await HttpClient.PostAsync(
            $"/api/v1.0/products/{product.ProductId}/reviews/{review.ReviewId}/helpful",
            null,
            ct
        );
        upvoteResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 8c. Customer requests return on delivered order item
        SetAuthToken(customerToken);
        var returnReq = new RequestReturnRequest(
            "Minor cosmetic scratch on headband",
            [new RequestReturnItemRequest(orderItemId, 1)]
        );
        var returnResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/orders/{order.OrderId}/returns",
            returnReq,
            ct
        );
        returnResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 8d. Customer lists returns for the order
        var orderReturnsResp = await HttpClient.GetAsync(
            $"/api/v1.0/orders/{order.OrderId}/returns",
            ct
        );
        orderReturnsResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var returnList = await orderReturnsResp.Content.ReadFromJsonAsync<
            IReadOnlyList<ReturnRequestDto>
        >(ct);
        returnList.ShouldNotBeNull();
        returnList.ShouldNotBeEmpty();
        var returnId = returnList[0].ReturnRequestId;

        // =========================================================================
        // STEP 9: Admin Reviews & Approves Return
        // =========================================================================
        SetAuthToken(adminToken);

        // 9a. Admin inspects pending returns
        var pendingReturnsResp = await HttpClient.GetAsync("/api/v1.0/returns/pending", ct);
        pendingReturnsResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pendingReturns = await pendingReturnsResp.Content.ReadFromJsonAsync<
            IReadOnlyList<ReturnRequestDto>
        >(ct);
        pendingReturns.ShouldNotBeNull();
        pendingReturns.ShouldContain(r => r.ReturnRequestId == returnId);

        // 9b. Admin approves the return
        var approveResp = await HttpClient.PutAsync(
            $"/api/v1.0/returns/{returnId}/approve",
            null,
            ct
        );
        approveResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 9c. Admin views sales report & dashboard metrics
        var metricsResp = await HttpClient.GetAsync(
            "/api/v1.0/admin/dashboard/metrics?period=30",
            ct
        );
        metricsResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        var topProductsResp = await HttpClient.GetAsync(
            "/api/v1.0/admin/reports/products/top?count=5",
            ct
        );
        topProductsResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
