using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Enums;
using Shouldly;
using Xunit;

namespace Shopizy.Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var firstName = "First";
        var lastName = "Last";
        var email = "test@example.com";
        var password = "hashed_password";
        var permissions = new List<PermissionId>();

        // Act
        var user = User.Create(
            firstName,
            lastName,
            email,
            password,
            UserRole.Customer,
            permissions
        );

        // Assert
        user.ShouldNotBeNull();
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.Email.ShouldBe(email);
        user.Password.ShouldBe(password);
        user.Role.ShouldBe(UserRole.Customer);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateUserDetailsAndAddress()
    {
        // Arrange
        var user = User.Create("Old", "User", "old@test.com", "hash", UserRole.Customer, []);
        var newFirst = "NewFirst";
        var newLast = "NewLast";

        // Act
        user.UpdateUserName(newFirst, newLast);
        user.UpdateAddress("Street", "City", "State", "Country", "12345");

        // Assert
        user.FirstName.ShouldBe(newFirst);
        user.LastName.ShouldBe(newLast);
        user.Address.ShouldNotBeNull();
        user.Address!.Street.ShouldBe("Street");
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePassword()
    {
        // Arrange
        var user = User.Create(
            "U",
            "U",
            "e@e.com",
            "old",
            UserRole.Customer,
            new List<PermissionId>()
        );
        var newPassword = "new_hashed_password";

        // Act
        user.UpdatePassword(newPassword);

        // Assert
        user.Password.ShouldBe(newPassword);
    }

    [Fact]
    public void UpdateRole_ShouldUpdateRole()
    {
        // Arrange
        var user = User.Create("U", "U", "e@e.com", "pass", UserRole.Customer, []);

        // Act
        user.UpdateRole(UserRole.Admin);

        // Assert
        user.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public void UpdatePhoneAndProfileImageUrlAndCustomerId_ShouldUpdateFields()
    {
        // Arrange
        var user = User.Create("U", "U", "e@e.com", "pass", UserRole.Customer, []);

        // Act
        user.UpdatePhone("1234567890");
        user.UpdateProfileImageUrl("https://example.com/img.png");
        user.UpdateCustomerId("cus_123");

        // Assert
        user.Phone.ShouldBe("1234567890");
        user.ProfileImageUrl.ShouldBe("https://example.com/img.png");
        user.CustomerId.ShouldBe("cus_123");
    }

    [Fact]
    public void AddressBook_AddUpdateRemoveSetDefault_ShouldManageAddressesCorrectly()
    {
        // Arrange
        var user = User.Create("U", "U", "e@e.com", "pass", UserRole.Customer, []);

        // Act - Add address
        var addr1 = user.AddAddress("Main St", "City1", "State1", "Country1", "10001", true);
        var addr2 = user.AddAddress("Second St", "City2", "State2", "Country2", "10002", false);

        // Assert
        user.Addresses.Count.ShouldBe(2);
        addr1.IsDefault.ShouldBeTrue();
        addr2.IsDefault.ShouldBeFalse();

        // Act - SetDefault
        user.SetDefaultAddress(addr2.Id);
        addr1.IsDefault.ShouldBeFalse();
        addr2.IsDefault.ShouldBeTrue();

        // Act - UpdateAddress
        var updateResult = user.UpdateAddress(
            addr2.Id,
            "Updated St",
            "City2",
            "State2",
            "Country2",
            "10002"
        );
        updateResult.IsError.ShouldBeFalse();
        addr2.Street.ShouldBe("Updated St");

        // Act - RemoveAddress
        var removeResult = user.RemoveAddress(addr1.Id);
        removeResult.IsError.ShouldBeFalse();
        user.Addresses.Count.ShouldBe(1);
    }

    [Fact]
    public void PasswordResetToken_SetValidateClear_ShouldBehaveCorrectly()
    {
        // Arrange
        var user = User.Create("U", "U", "e@e.com", "pass", UserRole.Customer, []);
        var token = "token123";
        var expiry = DateTime.UtcNow.AddHours(1);

        // Act
        user.SetPasswordResetToken(token, expiry);

        // Assert
        user.PasswordResetToken.ShouldBe(token);
        user.PasswordResetTokenExpiry.ShouldBe(expiry);
        user.IsPasswordResetTokenValid(token).ShouldBeTrue();
        user.IsPasswordResetTokenValid("wrongtoken").ShouldBeFalse();

        // Clear
        user.ClearPasswordResetToken();
        user.PasswordResetToken.ShouldBeNull();
        user.PasswordResetTokenExpiry.ShouldBeNull();
    }
}
