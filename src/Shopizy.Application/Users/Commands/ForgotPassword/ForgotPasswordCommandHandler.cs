using System.Security.Cryptography;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IConfiguration configuration
) : ICommandHandler<ForgotPasswordCommand, ErrorOr<string>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;

    public async Task<ErrorOr<string>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userRepository.GetUserByEmailAsync(command.Email);

        // Security best practice: return success even if user not found
        if (user is null)
        {
            return string.Empty;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTime.UtcNow.AddHours(1);

        user.SetPasswordResetToken(token, expiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var spaUrl =
            _configuration["SPAUrl"]?.TrimEnd('/')
            ?? _configuration["SpaUrl"]?.TrimEnd('/')
            ?? "http://localhost:4200";

        var encodedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{spaUrl}/auth/reset-password?resetToken={encodedToken}";

        var body = EmailTemplates.ForgotPassword.BuildBody(user.FirstName, resetUrl);

        await _emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.ForgotPassword.Subject,
            body: body,
            cancellationToken: cancellationToken
        );

        return token;
    }
}
