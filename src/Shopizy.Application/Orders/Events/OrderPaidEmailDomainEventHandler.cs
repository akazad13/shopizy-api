using Microsoft.Extensions.Configuration;
using Shopizy.Application.Common.EmailTemplates;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Orders.Events;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Orders.Events;

public class OrderPaidEmailDomainEventHandler(
    IUserRepository userRepository,
    IOrderRepository orderRepository,
    IEmailService emailService,
    IConfiguration configuration
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

        var order = await orderRepository.GetOrderByIdAsync(domainEvent.OrderId);
        var total = order?.GetTotal();
        var spaUrl =
            configuration["SPAUrl"]?.TrimEnd('/')
            ?? configuration["SpaUrl"]?.TrimEnd('/')
            ?? "https://shopizy.netlify.app";

        var body = EmailTemplates.OrderPaid.BuildBody(
            firstName: user.FirstName,
            orderId: domainEvent.OrderId.Value,
            totalAmount: total?.Amount,
            currency: total?.Currency.ToString(),
            deliveryMethod: order?.DeliveryMethod.ToString(),
            itemsCount: order?.OrderItems.Count,
            shippingCity: order?.ShippingAddress.City,
            shippingCountry: order?.ShippingAddress.Country,
            spaUrl: spaUrl
        );

        await emailService.SendAsync(
            to: user.Email,
            subject: EmailTemplates.OrderPaid.GetSubject(domainEvent.OrderId.Value),
            body: body,
            cancellationToken: cancellationToken
        );
    }
}
