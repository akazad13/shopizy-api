using System.Net;
using System.Net.Http.Json;
using Shopizy.Contracts.Admin;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.User;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Users;

public class UserAdminEndpointTests : BaseIntegrationTest
{
    public UserAdminEndpointTests(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Admin_GetUsers_WhenAdmin_ReturnsPagedResponse()
    {
        // 1. Authenticate as Admin
        await AuthenticateAsAdminAsync();

        // 2. Act
        var response = await HttpClient.GetAsync(
            "/api/v1.0/admin/users?pageNumber=1&pageSize=10",
            TestContext.Current.CancellationToken
        );

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<UserDetails>>(
            TestContext.Current.CancellationToken
        );
        pagedResponse.ShouldNotBeNull();
        pagedResponse.Items.ShouldNotBeNull();
        pagedResponse.Items.ShouldAllBe(u => !string.IsNullOrEmpty(u.Role));
    }

    [Fact]
    public async Task Admin_UpdateUserRole_WhenAdmin_ReturnsOk()
    {
        // 1. Create a regular user
        var (_, regularUserId) = await AuthenticateAsNewUserAsync(
            "TargetUser",
            "Test",
            "targetrole@example.com"
        );

        // 2. Authenticate as Admin
        await AuthenticateAsAdminAsync();

        // 3. Act
        var updateRoleRequest = new UpdateUserRoleRequest("Admin", new List<Guid>());
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/admin/users/{regularUserId}/role",
            updateRoleRequest,
            TestContext.Current.CancellationToken
        );

        // 4. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_UpdateUserRole_WithoutPermissionIds_WhenAdmin_ReturnsOk()
    {
        // 1. Create a regular user
        var (_, regularUserId) = await AuthenticateAsNewUserAsync(
            "TargetUser2",
            "Test",
            "targetrole2@example.com"
        );

        // 2. Authenticate as Admin
        await AuthenticateAsAdminAsync();

        // 3. Act - only send role, PermissionIds is null
        var updateRoleRequest = new UpdateUserRoleRequest("Admin");
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/admin/users/{regularUserId}/role",
            updateRoleRequest,
            TestContext.Current.CancellationToken
        );

        // 4. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
