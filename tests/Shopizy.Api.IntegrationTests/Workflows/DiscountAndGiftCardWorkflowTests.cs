using System.Net;
using System.Net.Http.Json;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.GiftCard;
using Shopizy.Contracts.PromoCode;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Workflows;

public class DiscountAndGiftCardWorkflowTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task PromoCodeAndGiftCard_LifecycleAndRedemption_SucceedsEndToEnd()
    {
        var ct = TestContext.Current.CancellationToken;

        // =========================================================================
        // STEP 1: Admin Sets Up Promo Code and Gift Card
        // =========================================================================
        var (adminToken, _) = await AuthenticateAsAdminAsync();

        // 1a. Create 15% discount promo code
        var promoCode = $"SUMMER-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
        var createPromoReq = new CreatePromoCodeRequest(
            Code: promoCode,
            Description: "Summer 15% Sale Discount",
            Discount: 15.00m,
            IsPercentage: true,
            IsActive: true
        );
        var promoResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/promo-codes",
            createPromoReq,
            ct
        );
        promoResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var promo = await promoResp.Content.ReadFromJsonAsync<PromoCodeResponse>(ct);
        promo.ShouldNotBeNull();
        promo.Code.ShouldBe(promoCode);

        // 1b. Create Gift Card with $75 initial balance
        var giftCardCode = $"GC-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        var createGiftCardReq = new CreateGiftCardRequest(
            Code: giftCardCode,
            InitialBalance: 75.00m,
            ExpiresOn: DateTime.UtcNow.AddYears(1)
        );
        var gcResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/gift-cards",
            createGiftCardReq,
            ct
        );
        gcResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var giftCard = await gcResp.Content.ReadFromJsonAsync<GiftCardResponse>(ct);
        giftCard.ShouldNotBeNull();
        giftCard.Code.ShouldBe(giftCardCode);
        giftCard.RemainingBalance.ShouldBe(75.00m);
        giftCard.IsActive.ShouldBeTrue();

        ClearAuthToken();

        // =========================================================================
        // STEP 2: Customer Validates and Redeems Discounts
        // =========================================================================
        var (customerToken, customerUserId) = await AuthenticateAsNewUserAsync("Bob", "Customer");

        // 2a. Validate promo code at checkout
        var validatePromoResp = await HttpClient.PostAsJsonAsync(
            $"/api/v1.0/users/{customerUserId}/orders/validate-promo",
            promoCode,
            ct
        );
        validatePromoResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var promoResult = await validatePromoResp.Content.ReadFromJsonAsync<PromoCodeResponse>(ct);
        promoResult.ShouldNotBeNull();
        promoResult.Discount.ShouldBe(15.00m);
        promoResult.IsPercentage.ShouldBeTrue();

        // 2b. Validate gift card
        var validateGcResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/gift-cards/validate",
            giftCardCode,
            ct
        );
        validateGcResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var gcValidation = await validateGcResp.Content.ReadFromJsonAsync<GiftCardResponse>(ct);
        gcValidation.ShouldNotBeNull();
        gcValidation.RemainingBalance.ShouldBe(75.00m);
        gcValidation.IsActive.ShouldBeTrue();

        // 2c. Redeem gift card
        var redeemGcReq = new RedeemGiftCardRequest(giftCardCode);
        var redeemGcResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/gift-cards/redeem",
            redeemGcReq,
            ct
        );
        redeemGcResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // =========================================================================
        // STEP 3: Admin Deactivates Gift Card and Cleans Up
        // =========================================================================
        SetAuthToken(adminToken);

        // 3a. Deactivate Gift Card
        var deactivateResp = await HttpClient.PatchAsync(
            $"/api/v1.0/admin/gift-cards/{giftCard.GiftCardId}/deactivate",
            null,
            ct
        );
        deactivateResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 3b. Delete Promo Code
        var deletePromoResp = await HttpClient.DeleteAsync(
            $"/api/v1.0/admin/promo-codes/{promo.PromoCodeId}",
            ct
        );
        deletePromoResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
