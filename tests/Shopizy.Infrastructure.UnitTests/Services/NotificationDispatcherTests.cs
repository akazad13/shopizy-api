using Microsoft.Extensions.Logging;
using Moq;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Infrastructure.Services.Notifications;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Services;

public class NotificationDispatcherTests
{
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<IPushNotificationService> _mockPushService = new();
    private readonly Mock<ILogger<NotificationDispatcher>> _mockLogger = new();
    private readonly NotificationDispatcher _sut;

    public NotificationDispatcherTests()
    {
        _sut = new NotificationDispatcher(
            _mockEmailService.Object,
            _mockPushService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task DispatchNotificationAsync_AllChannelsEnabled_ShouldSendToAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var prefs = new NotificationPreferencesDto(EmailEnabled: true, PushEnabled: true);

        // Act
        await _sut.DispatchNotificationAsync(
            userId,
            email,
            "Order Confirmation",
            "Thank you for your order!",
            null,
            prefs
        );

        // Assert
        _mockEmailService.Verify(
            e =>
                e.SendAsync(
                    email,
                    "Order Confirmation",
                    "Thank you for your order!",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockPushService.Verify(
            p =>
                p.SendPushNotificationAsync(
                    userId,
                    "Order Confirmation",
                    "Thank you for your order!",
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DispatchNotificationAsync_PushDisabled_ShouldOnlySendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var prefs = new NotificationPreferencesDto(EmailEnabled: true, PushEnabled: false);

        // Act
        await _sut.DispatchNotificationAsync(
            userId,
            email,
            "Price Drop",
            "Item is now on sale!",
            null,
            prefs
        );

        // Assert
        _mockEmailService.Verify(
            e =>
                e.SendAsync(
                    email,
                    "Price Drop",
                    "Item is now on sale!",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockPushService.Verify(
            p =>
                p.SendPushNotificationAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
