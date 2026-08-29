using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateUserRoleCommand, ErrorOr<Success>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ErrorOr<Success>> Handle(
        UpdateUserRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userRepository.GetUserByIdAsync(UserId.Create(command.UserId));
        if (user is null)
        {
            return (Error)CustomErrors.User.UserNotFound;
        }

        if (Enum.TryParse<UserRole>(command.Role, true, out var role))
        {
            user.UpdateRole(role);
        }

        if (command.PermissionIds is not null)
        {
            user.UpdatePermissions([.. command.PermissionIds.Select(PermissionId.Create)]);
        }

        return Result.Success;
    }
}
