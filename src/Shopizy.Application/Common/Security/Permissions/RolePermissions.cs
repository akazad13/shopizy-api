using Shopizy.Domain.Users.Enums;

namespace Shopizy.Application.Common.Security.Permissions;

/// <summary>
/// Defines the default permission sets associated with each user role.
/// </summary>
public static class RolePermissions
{
    /// <summary>
    /// Default permissions assigned to standard customers.
    /// </summary>
    public static readonly IReadOnlyList<string> Customer =
    [
        Permissions.Cart.Create,
        Permissions.Cart.Get,
        Permissions.Cart.Modify,
        Permissions.Cart.Delete,
        Permissions.Category.Get,
        Permissions.Product.Get,
        Permissions.Order.Create,
        Permissions.Order.Get,
        Permissions.Order.Modify,
        Permissions.Order.Delete,
        Permissions.User.Get,
        Permissions.User.Modify,
        Permissions.Wishlist.Create,
        Permissions.Wishlist.Get,
        Permissions.Wishlist.Modify,
        Permissions.Wishlist.Delete,
        Permissions.Review.Create,
        Permissions.Review.Get,
    ];

    /// <summary>
    /// Returns default permission names for a given role, or null if the role gets all permissions (e.g. Admin).
    /// </summary>
    public static IReadOnlyList<string>? GetDefaultPermissions(UserRole role) =>
        role switch
        {
            UserRole.Customer => Customer,
            UserRole.Admin => null, // Admin gets all permissions dynamically
            _ => Customer,
        };
}
