using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateUser;

/// <summary>
/// Represents a command to update user information.
/// </summary>
/// <param name="UserId">The unique identifier of the user to update.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="PhoneNumber">The user's phone number (optional).</param>
/// <param name="Street">The street address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state or province.</param>
/// <param name="Country">The country.</param>
/// <param name="ZipCode">The postal/ZIP code.</param>
public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Street,
    string? City,
    string? State,
    string? Country,
    string? ZipCode
) : ICommand<ErrorOr<Success>>, IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [CacheKeys.User(UserId)];
}
