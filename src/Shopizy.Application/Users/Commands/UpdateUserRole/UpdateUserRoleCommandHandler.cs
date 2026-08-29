using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Security.Permissions;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandler(
    IUserRepository userRepository,
    IPermissionLookup permissionLookup,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateUserRoleCommand, ErrorOr<Success>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPermissionLookup _permissionLookup = permissionLookup;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

            // If permissions not explicitly provided, update to the default permissions for the new role
            if (command.PermissionIds is null)
            {
                var permissionIds = role switch
                {
                    UserRole.Admin => await _permissionLookup.GetAllIdsAsync(cancellationToken),
                    _ => await _permissionLookup.GetIdsByNamesAsync(
                        RolePermissions.Customer,
                        cancellationToken
                    ),
                };

                user.UpdatePermissions([.. permissionIds]);
            }
        }

        if (command.PermissionIds is not null)
        {
            user.UpdatePermissions([.. command.PermissionIds.Select(PermissionId.Create)]);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
