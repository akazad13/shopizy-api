using ErrorOr;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Enums;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Auth.Commands.Register;

/// <summary>
/// Handles the <see cref="RegisterCommand"/> to create a new user account.
/// </summary>
/// <param name="userRepository"></param>
/// <param name="passwordManager"></param>
/// <param name="permissionLookup"></param>
/// <param name="unitOfWork"></param>
public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordManager passwordManager,
    IPermissionLookup permissionLookup,
    IUnitOfWork unitOfWork
) : ICommandHandler<RegisterCommand, ErrorOr<Success>>
{
    private static readonly string[] s_customerPermissionNames =
    [
        "create:cart",
        "create:order",
        "delete:cart",
        "delete:order",
        "get:cart",
        "get:order",
        "get:category",
        "get:product",
        "get:user",
        "modify:cart",
        "modify:order",
        "modify:user",
        "get:wishlist",
        "modify:wishlist",
        "create:wishlist",
        "create:review",
        "get:review",
    ];

    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordManager _passwordManager = passwordManager;
    private readonly IPermissionLookup _permissionLookup = permissionLookup;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the user registration process including validation, password hashing, and cart creation.
    /// </summary>
    /// <param name="command">The registration command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result or an error.</returns>
    public async Task<ErrorOr<Success>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        // check if command.Email is valid email address
        if (string.IsNullOrEmpty(command.Email) || !IsValidEmail(command.Email))
        {
            return (Error)CustomErrors.User.InvalidEmailFormat;
        }

        if (await _userRepository.GetUserByEmailAsync(command.Email) is not null)
        {
            return (Error)CustomErrors.User.DuplicateEmail;
        }

        if (
            string.IsNullOrEmpty(command.FirstName?.Trim())
            || string.IsNullOrEmpty(command.LastName?.Trim())
        )
        {
            return (Error)CustomErrors.User.InvalidName;
        }

        var hashedPassword = _passwordManager.CreateHashString(command.Password);

        var permissionIds = await _permissionLookup.GetIdsByNamesAsync(
            s_customerPermissionNames,
            cancellationToken
        );

        var user = User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            hashedPassword,
            UserRole.Customer,
            [.. permissionIds]
        );

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    // Helper method to validate email format
    static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
