using ErrorOr;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordManager passwordManager,
    IUnitOfWork unitOfWork
) : ICommandHandler<ResetPasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await userRepository.GetUserByResetTokenAsync(request.ResetToken);
        if (user is null)
        {
            return Error.NotFound(
                "PasswordReset.InvalidToken",
                "Invalid or expired password reset token."
            );
        }

        if (!user.IsPasswordResetTokenValid(request.ResetToken))
        {
            return Error.Validation(
                "PasswordReset.ExpiredToken",
                "The password reset token has expired."
            );
        }

        var hashedPassword = passwordManager.CreateHashString(request.NewPassword);
        user.UpdatePassword(hashedPassword);
        user.ClearPasswordResetToken();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
