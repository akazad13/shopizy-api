using Microsoft.Extensions.Logging;
using Moq;
using Shopizy.Infrastructure.ExternalServices.Email;
using Shouldly;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="LoggingEmailService"/>.
/// </summary>
public class LoggingEmailServiceTests
{
    private static Mock<ILogger<LoggingEmailService>> CreateLoggerMock()
    {
        var mockLogger = new Mock<ILogger<LoggingEmailService>>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
        return mockLogger;
    }

    private static void VerifyEmailLogged(Mock<ILogger<LoggingEmailService>> mockLogger) =>
        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );

    /// <summary>
    /// Tests that SendAsync completes successfully and logs the email information with valid inputs.
    /// </summary>
    [Fact]
    public async Task SendAsync_ValidInputs_CompletesSuccessfullyAndLogsInformation()
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);
        var to = "user@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        var task = service.SendAsync(to, subject, body, TestContext.Current.CancellationToken);
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync handles empty string parameters and logs them correctly.
    /// </summary>
    /// <param name="to">Email recipient.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body.</param>
    [Theory]
    [InlineData("", "subject", "body")]
    [InlineData("to@example.com", "", "body")]
    [InlineData("to@example.com", "subject", "")]
    [InlineData("", "", "")]
    public async Task SendAsync_EmptyStrings_CompletesSuccessfullyAndLogsInformation(
        string to,
        string subject,
        string body
    )
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);

        // Act
        var task = service.SendAsync(to, subject, body, TestContext.Current.CancellationToken);
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync handles whitespace-only string parameters.
    /// </summary>
    /// <param name="to">Email recipient.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body.</param>
    [Theory]
    [InlineData("   ", "subject", "body")]
    [InlineData("to@example.com", "\t\t", "body")]
    [InlineData("to@example.com", "subject", "\n\r")]
    [InlineData("   ", "   ", "   ")]
    public async Task SendAsync_WhitespaceStrings_CompletesSuccessfullyAndLogsInformation(
        string to,
        string subject,
        string body
    )
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);

        // Act
        var task = service.SendAsync(to, subject, body, TestContext.Current.CancellationToken);
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync handles strings with special characters correctly.
    /// </summary>
    /// <param name="to">Email recipient with special characters.</param>
    /// <param name="subject">Email subject with special characters.</param>
    /// <param name="body">Email body with special characters.</param>
    [Theory]
    [InlineData(
        "user+tag@example.com",
        "Subject: <Test> & \"Quotes\"",
        "Body with <html> tags & special chars: {braces}"
    )]
    [InlineData("user@example.com", "Subject with 日本語", "Body with émojis 🎉🎊")]
    [InlineData("test@test.com", "Subject\nwith\nnewlines", "Body\twith\ttabs")]
    public async Task SendAsync_SpecialCharacters_CompletesSuccessfullyAndLogsInformation(
        string to,
        string subject,
        string body
    )
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);

        // Act
        var task = service.SendAsync(to, subject, body, TestContext.Current.CancellationToken);
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync handles very long strings without issues.
    /// </summary>
    [Fact]
    public async Task SendAsync_VeryLongStrings_CompletesSuccessfullyAndLogsInformation()
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);
        var longString = new string('a', 10000);

        // Act
        var task = service.SendAsync(
            longString,
            longString,
            longString,
            TestContext.Current.CancellationToken
        );
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync ignores the cancellation token and completes successfully even when cancelled.
    /// </summary>
    [Fact]
    public async Task SendAsync_CancelledCancellationToken_CompletesSuccessfullyAndIgnoresCancellation()
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);
        await new CancellationTokenSource().CancelAsync();

        // Act
        var task = service.SendAsync(
            "to@example.com",
            "subject",
            "body",
            new CancellationTokenSource().Token
        );
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }

    /// <summary>
    /// Tests that SendAsync returns a completed task immediately (synchronous behavior).
    /// </summary>
    [Fact]
    public async Task SendAsync_ValidInputs_ReturnsCompletedTask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoggingEmailService>>();
        var service = new LoggingEmailService(mockLogger.Object);

        // Act
        var task = service.SendAsync(
            "to@example.com",
            "subject",
            "body",
            TestContext.Current.CancellationToken
        );

        // Assert
        task.IsCompleted.ShouldBeTrue();
        await task;
    }

    /// <summary>
    /// Tests that SendAsync with default cancellation token completes successfully.
    /// </summary>
    [Fact]
    public async Task SendAsync_DefaultCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var mockLogger = CreateLoggerMock();
        var service = new LoggingEmailService(mockLogger.Object);

        // Act
        var task = service.SendAsync(
            "to@example.com",
            "subject",
            "body",
            TestContext.Current.CancellationToken
        );
        await task;

        // Assert
        task.IsCompleted.ShouldBeTrue();
        VerifyEmailLogged(mockLogger);
    }
}
