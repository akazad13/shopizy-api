using Shopizy.Domain.Orders.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.Users.Queries.GetUser;

public record UserDto(
    UserId Id,
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
