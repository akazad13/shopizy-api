using ErrorOr;
using Moq;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Users.TestUtils;
using Shopizy.Application.Users.Commands.ResetPassword;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shouldly;

namespace Shopizy.Application.UnitTests.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordManager> _mockPasswordManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordManager = new Mock<IPasswordManager>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new ResetPasswordCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordManager.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundByToken_ShouldReturnInvalidTokenError()
    {
        // Arrange
        var command = new ResetPasswordCommand("invalid-token", "NewPassword123!");

        _mockUserRepository
            .Setup(r => r.GetUserByResetTokenAsync(command.ResetToken))
            .ReturnsAsync((Shopizy.Domain.Users.User?)null);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("PasswordReset.InvalidToken");
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ShouldReturnExpiredTokenError()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var expiredToken = "expired-token";
        user.SetPasswordResetToken(expiredToken, DateTime.UtcNow.AddHours(-1));
        var command = new ResetPasswordCommand(expiredToken, "NewPassword123!");

        _mockUserRepository.Setup(r => r.GetUserByResetTokenAsync(expiredToken)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("PasswordReset.ExpiredToken");
    }

    [Fact]
    public async Task Handle_WhenTokenValid_ShouldUpdatePasswordClearTokenAndReturnSuccess()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var validToken = "valid-token";
        user.SetPasswordResetToken(validToken, DateTime.UtcNow.AddHours(1));
        var command = new ResetPasswordCommand(validToken, "NewPassword123!");

        _mockUserRepository.Setup(r => r.GetUserByResetTokenAsync(validToken)).ReturnsAsync(user);
        _mockPasswordManager
            .Setup(p => p.CreateHashString("NewPassword123!"))
            .Returns("hashed_new_password");

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);
        user.Password.ShouldBe("hashed_new_password");
        user.PasswordResetToken.ShouldBeNull();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
