using System.Net;
using System.Net.Http.Json;
using Shopizy.Contracts.Category;
using Shopizy.Contracts.Order;
using Shopizy.Contracts.Product;
using Shopizy.Contracts.Returns;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Workflows;

public class OrderLifecycleAndCancellationE2ETests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    private async Task<(Guid CategoryId, Guid ProductId)> SetupProductAsync(int stockQuantity = 50)
    {
        await AuthenticateAsAdminAsync();

        var catResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest($"CancelCat-{Guid.NewGuid().ToString()[..4]}", null),
            TestContext.Current.CancellationToken
        );
        catResponse.EnsureSuccessStatusCode();
        var category = await catResponse.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );

        var prodResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/products",
            new CreateProductRequest(
                $"CancelProd-{Guid.NewGuid().ToString()[..4]}",
                "Short description",
                "Full description",
                category!.Id,
                45.00m,
                1,
                0m,
                $"CAN-{Guid.NewGuid().ToString()[..6]}",
                null,
                "Blue",
                "Medium",
                "tag",
                Guid.NewGuid().ToString()[..8],
                stockQuantity,
                null
            ),
            TestContext.Current.CancellationToken
        );
        prodResponse.EnsureSuccessStatusCode();
        var product = await prodResponse.Content.ReadFromJsonAsync<ProductResponse>(
            TestContext.Current.CancellationToken
        );

        return (category.Id, product!.ProductId);
    }

    [Fact]
    public async Task OrderPlacement_ThenCancellation_UpdatesStatusToCancelled()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, productId) = await SetupProductAsync();

        // 1. Customer places order
        var (_, userId) = await AuthenticateAsNewUserAsync("Cancel", "User");
        var orderItems = new List<OrderItemRequest> { new(productId, "Blue", "Medium", 1) };
        var checkoutRequest = new CreateOrderRequest(
            PromoCode: "",
            GiftCardCode: null,
            DeliveryMethod: 1,
            DeliveryCharge: new Price(5.00m, "USD"),
            OrderItems: orderItems,
            ShippingAddress: new Address("123 Cancel Rd", "Seattle", "WA", "USA", "98101")
        );

        var placeResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/orders/checkout",
            checkoutRequest,
            ct
        );
        placeResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var order = await placeResp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        order.ShouldNotBeNull();
        order.OrderStatus.ShouldBe("Pending");

        // 2. Customer cancels order
        var cancelReq = new CancelOrderRequest("Placed order by mistake");
        var cancelResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{userId}/orders/{order.OrderId}/cancel",
            cancelReq,
            ct
        );
        cancelResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 3. Verify status updated to Cancelled
        var getOrderResp = await HttpClient.GetAsync(
            $"/api/v1.0/users/{userId}/orders/{order.OrderId}",
            ct
        );
        getOrderResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedOrder = await getOrderResp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.OrderStatus.ShouldBe("Cancelled");
    }

    [Fact]
    public async Task ReturnRequest_AdminRejection_UpdatesReturnStatusWithAdminNote()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, productId) = await SetupProductAsync();
        var (adminToken, _) = await AuthenticateAsAdminAsync();

        // 1. Customer places order
        var (customerToken, userId) = await AuthenticateAsNewUserAsync("Return", "RejectUser");
        var orderItems = new List<OrderItemRequest> { new(productId, "Blue", "Medium", 1) };
        var checkoutRequest = new CreateOrderRequest(
            PromoCode: "",
            GiftCardCode: null,
            DeliveryMethod: 1,
            DeliveryCharge: new Price(5.00m, "USD"),
            OrderItems: orderItems,
            ShippingAddress: new Address("456 Return Way", "Seattle", "WA", "USA", "98101")
        );

        var placeResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/orders/checkout",
            checkoutRequest,
            ct
        );
        placeResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var order = await placeResp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        order.ShouldNotBeNull();
        var orderItemId = order.OrderItems[0].OrderItemId;

        // 2. Admin marks delivered
        SetAuthToken(adminToken);
        await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/admin/orders/{order.OrderId}/status",
            (int)Domain.Orders.Enums.OrderStatus.Delivered,
            ct
        );

        // 3. Customer requests return
        SetAuthToken(customerToken);
        var returnReq = new RequestReturnRequest(
            "Did not fit well",
            [new RequestReturnItemRequest(orderItemId, 1)]
        );
        var returnResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/orders/{order.OrderId}/returns",
            returnReq,
            ct
        );
        returnResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        var orderReturnsResp = await HttpClient.GetAsync(
            $"/api/v1.0/orders/{order.OrderId}/returns",
            ct
        );
        var returnList = await orderReturnsResp.Content.ReadFromJsonAsync<
            IReadOnlyList<ReturnRequestDto>
        >(ct);
        returnList.ShouldNotBeNull();
        var returnId = returnList[0].ReturnRequestId;

        // 4. Admin rejects the return
        SetAuthToken(adminToken);
        var rejectReq = new RejectReturnRequest("Return window of 30 days exceeded.");
        var rejectResp = await HttpClient.PutAsJsonAsync(
            $"/api/v1.0/returns/{returnId}/reject",
            rejectReq,
            ct
        );
        rejectResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 5. Customer checks return status
        SetAuthToken(customerToken);
        var singleReturnResp = await HttpClient.GetAsync($"/api/v1.0/returns/{returnId}", ct);
        singleReturnResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var returnDetails = await singleReturnResp.Content.ReadFromJsonAsync<ReturnRequestDto>(ct);
        returnDetails.ShouldNotBeNull();
        returnDetails.Status.ShouldBe("Rejected");
        returnDetails.AdminNote.ShouldNotBeNull();
        returnDetails.AdminNote.ShouldContain("exceeded");
    }

    [Fact]
    public async Task BulkUpdateOrderStatus_UpdatesMultipleOrdersSuccessfully()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, productId) = await SetupProductAsync();
        var (adminToken, _) = await AuthenticateAsAdminAsync();

        // Customer places 2 orders
        var (_, userId) = await AuthenticateAsNewUserAsync("Bulk", "User");
        var order1Id = await PlaceOrderAsync([
            new
            {
                productId,
                color = "Blue",
                size = "Medium",
                quantity = 1,
            },
        ]);
        var order2Id = await PlaceOrderAsync([
            new
            {
                productId,
                color = "Blue",
                size = "Medium",
                quantity = 1,
            },
        ]);

        // Admin bulk updates both orders to Processing (2)
        SetAuthToken(adminToken);
        var bulkReq = new BulkUpdateOrderStatusRequest([order1Id, order2Id], 2);
        var bulkResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/orders/bulk-status",
            bulkReq,
            ct
        );
        bulkResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify both orders are now Processing via Admin order endpoint
        var o1Resp = await HttpClient.GetAsync($"/api/v1.0/admin/orders/{order1Id}", ct);
        o1Resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var o1 = await o1Resp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        o1!.OrderStatus.ShouldBe("Processing");

        var o2Resp = await HttpClient.GetAsync($"/api/v1.0/admin/orders/{order2Id}", ct);
        o2Resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var o2 = await o2Resp.Content.ReadFromJsonAsync<OrderDetailResponse>(ct);
        o2!.OrderStatus.ShouldBe("Processing");
    }
}
