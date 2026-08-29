using Shopizy.Contracts.Order;

namespace Shopizy.Contracts.User;

/// <summary>
/// Represents detailed user information.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Role">The user's role.</param>
/// <param name="ProfileImageUrl">The URL of the user's profile image.</param>
/// <param name="Phone">The user's phone number.</param>
/// <param name="Address">The user's address.</param>
/// <param name="TotalOrders">The total number of orders placed by the user.</param>
/// <param name="TotalReviewed">The total number of products reviewed by the user.</param>
/// <param name="TotalFavorites">The total number of products favorited by the user.</param>
/// <param name="TotalReturns">The total number of returned orders.</param>
/// <param name="CreatedOn">The date and time when the user account was created.</param>
/// <param name="ModifiedOn">The date and time when the user account was last modified.</param>
public record UserDetails(
    Guid Id,
    string? FirstName,
    string? LastName,
    string Email,
    string Role,
    string? ProfileImageUrl,
    string? Phone,
    Address? Address,
    int TotalOrders,
    int TotalReviewed,
    int TotalFavorites,
    int TotalReturns,
    DateTime CreatedOn,
    DateTime? ModifiedOn
);
