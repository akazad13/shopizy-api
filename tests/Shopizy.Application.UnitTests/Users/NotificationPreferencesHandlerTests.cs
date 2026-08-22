using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Users.Commands.UpdateNotificationPreferences;
using Shopizy.Application.Users.Queries.GetNotificationPreferences;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shouldly;
using Xunit;

namespace Shopizy.Application.UnitTests.Users;

public class NotificationPreferencesHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;

    public NotificationPreferencesHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task GetNotificationPreferences_WhenUserNotFound_ShouldReturnUserNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.GetUserByIdAsync(It.Is<UserId>(id => id.Value == userId)))
            .ReturnsAsync((User?)null);

        var handler = new GetNotificationPreferencesQueryHandler(_mockUserRepository.Object);
        var result = await handler.Handle(
            new GetNotificationPreferencesQuery(userId),
            CancellationToken.None
        );

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(CustomErrors.User.UserNotFound);
    }

    [Fact]
    public async Task GetNotificationPreferences_WhenUserExists_ShouldReturnPreferences()
    {
        var user = User.Create("First", "Last", "test@example.com", "hash", UserRole.Customer, []);
        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var handler = new GetNotificationPreferencesQueryHandler(_mockUserRepository.Object);
        var result = await handler.Handle(
            new GetNotificationPreferencesQuery(user.Id.Value),
            CancellationToken.None
        );

        result.IsError.ShouldBeFalse();
        result.Value.EmailEnabled.ShouldBeTrue();
        result.Value.SmsEnabled.ShouldBeTrue();
        result.Value.PushEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenUserNotFound_ShouldReturnUserNotFound()
    {
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.GetUserByIdAsync(It.Is<UserId>(id => id.Value == userId)))
            .ReturnsAsync((User?)null);

        var handler = new UpdateNotificationPreferencesCommandHandler(_mockUserRepository.Object);
        var command = new UpdateNotificationPreferencesCommand(
            userId,
            EmailEnabled: false,
            SmsEnabled: false,
            PushEnabled: false,
            OrderUpdates: false,
            Promotions: false,
            PriceAlerts: false,
            RestockAlerts: false
        );
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(CustomErrors.User.UserNotFound);
    }

    [Fact]
    public async Task UpdateNotificationPreferences_WhenUserExists_ShouldUpdateAndReturnNewPreferences()
    {
        var user = User.Create("First", "Last", "test@example.com", "hash", UserRole.Customer, []);
        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var handler = new UpdateNotificationPreferencesCommandHandler(_mockUserRepository.Object);
        var command = new UpdateNotificationPreferencesCommand(
            user.Id.Value,
            EmailEnabled: false,
            SmsEnabled: true,
            PushEnabled: false,
            OrderUpdates: true,
            Promotions: false,
            PriceAlerts: true,
            RestockAlerts: false
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.EmailEnabled.ShouldBeFalse();
        result.Value.SmsEnabled.ShouldBeTrue();
        result.Value.PushEnabled.ShouldBeFalse();
        result.Value.OrderUpdates.ShouldBeTrue();
        result.Value.Promotions.ShouldBeFalse();
        result.Value.PriceAlerts.ShouldBeTrue();
        result.Value.RestockAlerts.ShouldBeFalse();

        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
    }
}
