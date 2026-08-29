using Microsoft.Extensions.Logging;
using Moq;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Infrastructure.Services.Notifications;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Services;

public class NotificationDispatcherTests
{
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<ILogger<NotificationDispatcher>> _mockLogger = new();
    private readonly NotificationDispatcher _sut;

    public NotificationDispatcherTests()
    {
        _sut = new NotificationDispatcher(_mockEmailService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task DispatchNotificationAsync_EmailEnabled_ShouldSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var prefs = new NotificationPreferencesDto(EmailEnabled: true);

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
    }

    [Fact]
    public async Task DispatchNotificationAsync_EmailDisabled_ShouldNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var prefs = new NotificationPreferencesDto(EmailEnabled: false);

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
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
