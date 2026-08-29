using System.Net;
using Shopizy.Contracts.User;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Users;

public class GetUserTests : BaseIntegrationTest
{
    public GetUserTests(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUser_WhenAuthenticated_ReturnsUser()
    {
        // Arrange
        var (token, userId) = await AuthenticateAsNewUserAsync();

        // Act
        var response = await HttpClient.GetAsync(
            $"/api/v1.0/users/{userId}",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var userDetails = await response.Content.ReadFromJsonAsync<UserDetails>(
            TestContext.Current.CancellationToken
        );
        userDetails.ShouldNotBeNull();
        userDetails.Id.ShouldBe(userId);
        userDetails.Role.ShouldBe("Customer");
    }

    [Fact]
    public async Task GetUser_WhenUnauthorized_ReturnsForbidden()
    {
        // Arrange
        var (_, user1Id) = await AuthenticateAsNewUserAsync("User1", "Test", "user1@test.com");
        var (_, user2Id) = await AuthenticateAsNewUserAsync("User2", "Test", "user2@test.com");

        // We are currently authenticated as user2. Try to access user1.

        // Act
        var response = await HttpClient.GetAsync(
            $"/api/v1.0/users/{user1Id}",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var (_, userId) = await AuthenticateAsNewUserAsync("Test", "User", "test@example.com");
        var updateRequest = new UpdateUserRequest(
            "Updated",
            "Name",
            "1234567890",
            new UpdateAddressRequest { Street = "123 St" }
        );

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{userId}",
            updateRequest,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrders_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var (_, userId) = await AuthenticateAsNewUserAsync();

        // Act
        var response = await HttpClient.GetAsync(
            $"/api/v1.0/users/{userId}/orders",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePassword_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var password = "Password123!";
        var (token, userId) = await AuthenticateAsNewUserAsync(
            "Test",
            "User",
            "pass@test.com",
            password
        );
        var updateRequest = new UpdatePasswordRequest
        {
            OldPassword = password,
            NewPassword = "NewPassword123!",
        };

        // Act
        var response = await HttpClient.PatchAsJsonAsync(
            $"/api/v1.0/users/{userId}/password",
            updateRequest,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
