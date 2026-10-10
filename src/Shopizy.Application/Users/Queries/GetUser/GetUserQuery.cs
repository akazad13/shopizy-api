using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Queries.GetUser;

/// <summary>
/// Represents a query to retrieve user information by user ID.
/// </summary>
/// <param name="UserId">The unique identifier of the user to retrieve.</param>
public record GetUserQuery(Guid UserId) : IQuery<ErrorOr<UserDto>>, ICachableRequest
{
    public string CacheKey => CacheKeys.User(UserId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(60);
}
