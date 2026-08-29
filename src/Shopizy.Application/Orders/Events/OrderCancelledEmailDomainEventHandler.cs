using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Orders.Events;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Orders.Events;

public class OrderCancelledEmailDomainEventHandler(
    IUserRepository userRepository,
    IEmailService emailService
) : IDomainEventHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(
        OrderCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var order = domainEvent.Order;
        var user = await userRepository.GetUserByIdAsync(order.UserId);
        if (user is null)
        {
            return;
        }

        await emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.OrderCancelled.GetSubject(order.Id.Value),
            body: EmailTemplates.OrderCancelled.BuildBody(
                user.FirstName,
                order.Id.Value,
                order.CancellationReason
            ),
            cancellationToken: cancellationToken
        );
    }
}
