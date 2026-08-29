using ErrorOr;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Notifications.Commands.SendEmail;

public class SendEmailCommandHandler(IEmailService emailService)
    : ICommandHandler<SendEmailCommand, ErrorOr<bool>>
{
    private readonly IEmailService _emailService = emailService;

    public async Task<ErrorOr<bool>> Handle(
        SendEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        await _emailService.SendAsync(request.To, request.Subject, request.Body, cancellationToken);

        return true;
    }
}
