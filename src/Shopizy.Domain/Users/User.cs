using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Orders.ValueObjects;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.ProductReviews.ValueObjects;
using Shopizy.Domain.Users.Entities;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public sealed class User : AggregateRoot<UserId, Guid>, IAuditable
{
    private readonly List<OrderId> _orderIds = [];
    private readonly List<ProductReviewId> _productReviewIds = [];
    private readonly List<PermissionId> _permissionIds = [];
    private readonly List<UserAddress> _addresses = [];

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the user's role.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Gets the user's profile image URL.
    /// </summary>
    public string? ProfileImageUrl { get; private set; }

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Gets the credential information for this user (password, reset token, 2FA).
    /// </summary>
    public required UserCredential Credentials { get; init; }

    /// <summary>Gets the user's hashed password.</summary>
    public string? Password => Credentials.Password;

    /// <summary>Gets the password reset token.</summary>
    public string? PasswordResetToken => Credentials.PasswordResetToken;

    /// <summary>Gets the password reset token expiry.</summary>
    public DateTime? PasswordResetTokenExpiry => Credentials.PasswordResetTokenExpiry;

    /// <summary>Gets the two-factor authentication secret.</summary>
    public string? TwoFactorSecret => Credentials.TwoFactorSecret;

    /// <summary>Gets whether two-factor authentication is enabled.</summary>
    public bool IsTwoFactorEnabled => Credentials.IsTwoFactorEnabled;

    /// <summary>
    /// Gets the user's customer ID for payment processing.
    /// </summary>
    public string? CustomerId { get; private set; }

    /// <summary>
    /// Gets the user's notification preferences.
    /// </summary>
    public NotificationPreference NotificationPreferences { get; private set; } =
        NotificationPreference.CreateDefault();

    /// <summary>
    /// Gets the user's address.
    /// </summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTime CreatedOn { get; }

    /// <summary>
    /// Gets the date and time when the user was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; }

    /// <summary>
    /// Gets the read-only list of order IDs associated with this user.
    /// </summary>
    public IReadOnlyList<OrderId> OrderIds => _orderIds.AsReadOnly();

    /// <summary>
    /// Gets the read-only list of product review IDs created by this user.
    /// </summary>
    public IReadOnlyList<ProductReviewId> ProductReviewIds => _productReviewIds.AsReadOnly();

    /// <summary>
    /// Gets the read-only list of permission IDs assigned to this user.
    /// </summary>
    public IReadOnlyList<PermissionId> PermissionIds => _permissionIds.AsReadOnly();

    /// <summary>
    /// Gets the read-only list of addresses associated with this user.
    /// </summary>
    public IReadOnlyList<UserAddress> Addresses => _addresses.AsReadOnly();

    /// <summary>
    /// Creates a new user instance.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's hashed password.</param>
    /// <param name="role">The user's role.</param>
    /// <param name="permissionIds">The list of permission IDs to assign to the user.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    public static User Create(
        string firstName,
        string lastName,
        string email,
        string? password,
        UserRole role,
        IList<PermissionId> permissionIds
    )
    {
        var user = new User
        {
            Id = UserId.CreateUnique(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Credentials = new UserCredential(password),
            Role = role,
        };

        user._permissionIds.AddRange(permissionIds);
        user.AddDomainEvent(new Events.UserRegisteredDomainEvent(user));

        return user;
    }

    private User() { }

    /// <summary>
    /// Updates the user's address information.
    /// </summary>
    /// <param name="street">The street address.</param>
    /// <param name="city">The city.</param>
    /// <param name="state">The state or province.</param>
    /// <param name="country">The country.</param>
    /// <param name="zipCode">The postal/ZIP code.</param>
    public void UpdateAddress(
        string street,
        string city,
        string state,
        string country,
        string zipCode
    ) => Address = Address.CreateNew(street, city, state, country, zipCode);

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    /// <param name="password">The new hashed password.</param>
    public void UpdatePassword(string password) => Credentials.UpdatePassword(password);

    /// <summary>
    /// Updates the user's name.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    public void UpdateUserName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Updates the user's phone.
    /// </summary>
    /// <param name="phone">The user's phone.</param>
    public void UpdatePhone(string phone) => Phone = phone;

    /// <summary>
    /// Updates the user's profile picture.
    /// </summary>
    /// <param name="profileImageUrl">The user's profile picture.</param>
    public void UpdateProfileImageUrl(string profileImageUrl) => ProfileImageUrl = profileImageUrl;

    /// <summary>
    /// Updates the user's customer ID for payment processing.
    /// </summary>
    /// <param name="customerId">The customer ID from the payment provider.</param>
    public void UpdateCustomerId(string customerId) => CustomerId = customerId;

    /// <summary>
    /// Updates the user's role.
    /// </summary>
    /// <param name="role">The new role.</param>
    public void UpdateRole(UserRole role) => Role = role;

    /// <summary>
    /// Updates the user's permissions.
    /// </summary>
    /// <param name="permissionIds">The new list of permission IDs.</param>
    public void UpdatePermissions(IList<PermissionId> permissionIds)
    {
        _permissionIds.Clear();
        _permissionIds.AddRange(permissionIds);
    }

    /// <summary>
    /// Updates the user's notification preferences.
    /// </summary>
    public void UpdateNotificationPreferences(
        bool emailEnabled,
        bool orderUpdates,
        bool promotions,
        bool priceAlerts,
        bool restockAlerts
    )
    {
        NotificationPreferences ??= NotificationPreference.CreateDefault();
        NotificationPreferences.Update(
            emailEnabled,
            orderUpdates,
            promotions,
            priceAlerts,
            restockAlerts
        );
    }

    /// <summary>
    /// Adds a new address to the user's address book.
    /// </summary>
    /// <param name="street"></param>
    /// <param name="city"></param>
    /// <param name="state"></param>
    /// <param name="country"></param>
    /// <param name="zipCode"></param>
    /// <param name="isDefault"></param>
    public UserAddress AddAddress(
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        bool isDefault
    )
    {
        if (isDefault)
        {
            foreach (var a in _addresses)
                a.SetDefault(false);
        }

        var address = UserAddress.Create(street, city, state, country, zipCode, isDefault);
        _addresses.Add(address);
        return address;
    }

    /// <summary>
    /// Updates an existing address.
    /// </summary>
    /// <param name="addressId"></param>
    /// <param name="street"></param>
    /// <param name="city"></param>
    /// <param name="state"></param>
    /// <param name="country"></param>
    /// <param name="zipCode"></param>
    public DomainResult<UserAddress> UpdateAddress(
        UserAddressId addressId,
        string street,
        string city,
        string state,
        string country,
        string zipCode
    )
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return CustomErrors.UserAddress.AddressNotFound;
        address.Update(street, city, state, country, zipCode);
        return address;
    }

    /// <summary>
    /// Removes an address from the user's address book.
    /// </summary>
    /// <param name="addressId"></param>
    public DomainResult<bool> RemoveAddress(UserAddressId addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return CustomErrors.UserAddress.AddressNotFound;
        _addresses.Remove(address);
        return true;
    }

    /// <summary>
    /// Sets the default address for the user.
    /// </summary>
    /// <param name="addressId"></param>
    public DomainResult<bool> SetDefaultAddress(UserAddressId addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return CustomErrors.UserAddress.AddressNotFound;
        foreach (var a in _addresses)
            a.SetDefault(false);
        address.SetDefault(true);
        return true;
    }

    /// <summary>Sets the password reset token and expiry.</summary>
    /// <param name="token"></param>
    /// <param name="expiry"></param>
    public void SetPasswordResetToken(string token, DateTime expiry) =>
        Credentials.SetPasswordResetToken(token, expiry);

    /// <summary>Returns true when the reset token matches and has not expired.</summary>
    /// <param name="token"></param>
    public bool IsPasswordResetTokenValid(string token) =>
        Credentials.IsPasswordResetTokenValid(token);

    /// <summary>Clears the password reset token after use.</summary>
    public void ClearPasswordResetToken() => Credentials.ClearPasswordResetToken();

    /// <summary>Generates a new TOTP secret and marks 2FA as pending confirmation.</summary>
    public string EnableTwoFactor() => Credentials.EnableTwoFactor();

    /// <summary>Marks 2FA as fully enabled after code verification.</summary>
    public void ConfirmTwoFactor() => Credentials.ConfirmTwoFactor();

    /// <summary>Removes the TOTP secret and disables 2FA.</summary>
    public void DisableTwoFactor() => Credentials.DisableTwoFactor();
}
