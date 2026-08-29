using System.Net;
using System.Net.Http.Json;
using Shopizy.Contracts.Authentication;
using Shopizy.Contracts.Cart;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.User;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Workflows;

public class SecurityAndAccessControlWorkflowTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task WeakPasswordRegistration_IsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var weakReq = new RegisterRequest(
            "Weak",
            "User",
            $"weak-{Guid.NewGuid().ToString()[..6]}@test.com",
            "12345"
        );
        var resp = await HttpClient.PostAsJsonAsync("/api/v1.0/auth/register", weakReq, ct);
        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PasswordRotation_OldPasswordBecomesInvalid_NewPasswordWorks()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"rotate-{Guid.NewGuid().ToString()[..8]}@test.com";
        var oldPassword = "InitialPassword@1234!";
        var newPassword = "UpdatedPassword@9876!";

        var (token, userId) = await AuthenticateAsNewUserAsync(
            "Rotate",
            "User",
            email,
            oldPassword
        );

        // 1. Rotate password
        var updatePassReq = new UpdatePasswordRequest
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
        };
        var updateResp = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{userId}/password",
            updatePassReq,
            ct
        );
        updateResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        ClearAuthToken();

        // 2. Attempt login with old password -> should fail (401 Unauthorized)
        var oldLoginResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/auth/login",
            new LoginRequest(email, oldPassword),
            ct
        );
        oldLoginResp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // 3. Login with new password -> should succeed (200 OK)
        var newLoginResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/auth/login",
            new LoginRequest(email, newPassword),
            ct
        );
        newLoginResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrossTenantAccess_IsStrictlyForbidden()
    {
        var ct = TestContext.Current.CancellationToken;
        var (tokenUserA, userIdA) = await AuthenticateAsNewUserAsync("Alice", "TenantA");
        var (_, userIdB) = await AuthenticateAsNewUserAsync("Bob", "TenantB");

        // User B tries to read User A's cart
        var cartResp = await HttpClient.GetAsync($"/api/v1.0/users/{userIdA}/cart", ct);
        cartResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // User B tries to read User A's orders
        var ordersResp = await HttpClient.GetAsync($"/api/v1.0/users/{userIdA}/orders", ct);
        ordersResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // User B tries to read User A's addresses
        var addrResp = await HttpClient.GetAsync($"/api/v1.0/users/{userIdA}/addresses", ct);
        addrResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // User B tries to read User A's notification preferences
        var notifResp = await HttpClient.GetAsync(
            $"/api/v1.0/users/{userIdA}/notification-preferences",
            ct
        );
        notifResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // User B tries to access admin endpoint -> 403 Forbidden
        var adminUsersResp = await HttpClient.GetAsync("/api/v1.0/admin/users", ct);
        adminUsersResp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SecurityHeadersAndCorrelationId_ArePresentInResponses()
    {
        var ct = TestContext.Current.CancellationToken;
        var correlationId = "corr-test-e2e-998877";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/categories");
        request.Headers.Add("X-Correlation-Id", correlationId);

        var response = await HttpClient.SendAsync(request, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Correlation ID header should be echoed back
        response.Headers.Contains("X-Correlation-Id").ShouldBeTrue();
        response.Headers.GetValues("X-Correlation-Id").First().ShouldBe(correlationId);

        // Security headers should be present
        response.Headers.Contains("X-Content-Type-Options").ShouldBeTrue();
        response.Headers.GetValues("X-Content-Type-Options").First().ShouldBe("nosniff");
    }
}
