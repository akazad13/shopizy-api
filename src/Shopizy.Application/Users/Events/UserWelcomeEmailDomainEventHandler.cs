using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Users.Events;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Events;

public class UserWelcomeEmailDomainEventHandler(IEmailService emailService)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(
        UserRegisteredDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var user = domainEvent.User;

        await emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.Welcome.Subject,
            body: EmailTemplates.Welcome.BuildBody(user.FirstName),
            cancellationToken: cancellationToken
        );
    }
}
