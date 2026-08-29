using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Orders.Events;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Orders.Events;

public class OrderPaidEmailDomainEventHandler(
    IUserRepository userRepository,
    IEmailService emailService
) : IDomainEventHandler<PaymentCompletedDomainEvent>
{
    public async Task Handle(
        PaymentCompletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var user = await userRepository.GetUserByIdAsync(domainEvent.UserId);
        if (user is null)
        {
            return;
        }

        await emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.OrderPaid.GetSubject(domainEvent.OrderId.Value),
            body: EmailTemplates.OrderPaid.BuildBody(user.FirstName, domainEvent.OrderId.Value),
            cancellationToken: cancellationToken
        );
    }
}
