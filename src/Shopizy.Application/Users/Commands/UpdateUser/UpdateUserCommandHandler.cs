using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateUser;

/// <summary>
/// Handles the <see cref="UpdateUserCommand"/> to update user information.
/// </summary>
/// <param name="userRepository"></param>
public class UpdateUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateUserCommand, ErrorOr<Success>>
{
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Handles the update user command.
    /// </summary>
    /// <param name="request">The update user command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result or an error.</returns>
    public async Task<ErrorOr<Success>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetUserByIdAsync(UserId.Create(request.UserId));
        if (user is null)
        {
            return (Error)CustomErrors.User.UserNotFound;
        }

        user.UpdateUserName(request.FirstName, request.LastName);

        if (
            request.Street is not null
            || request.City is not null
            || request.State is not null
            || request.Country is not null
            || request.ZipCode is not null
        )
        {
            user.UpdateAddress(
                request.Street ?? string.Empty,
                request.City ?? string.Empty,
                request.State ?? string.Empty,
                request.Country ?? string.Empty,
                request.ZipCode ?? string.Empty
            );
        }

        return Result.Success;
    }
}
