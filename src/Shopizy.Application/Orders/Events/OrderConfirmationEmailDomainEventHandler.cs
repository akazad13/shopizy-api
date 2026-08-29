using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Orders.Events;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Orders.Events;

public class OrderConfirmationEmailDomainEventHandler(
    IUserRepository userRepository,
    IEmailService emailService
) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(
        OrderCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var order = domainEvent.Order;
        var user = await userRepository.GetUserByIdAsync(order.UserId);

        if (user is null)
        {
            return;
        }

        var total = order.GetTotal();

        await emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.OrderConfirmation.GetSubject(order.Id.Value),
            body: EmailTemplates.OrderConfirmation.BuildBody(
                user.FirstName,
                order.Id.Value,
                total.Amount,
                total.Currency.ToString()
            ),
            cancellationToken: cancellationToken
        );
    }
}
