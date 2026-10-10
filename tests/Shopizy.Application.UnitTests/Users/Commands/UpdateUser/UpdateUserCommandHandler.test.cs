using ErrorOr;
using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Users.TestUtils;
using Shopizy.Application.Users.Commands.UpdateUser;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Caching;

namespace Shopizy.Application.UnitTests.Users.Commands.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly UpdateUserCommandHandler _sut;
    private readonly Mock<IUserRepository> _mockUserRepository;

    public UpdateUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _sut = new UpdateUserCommandHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Should_UpdateUserSuccessfully_When_ValidDataProvided()
    {
        // Arrange
        var command = UpdateUserCommandUtils.CreateCommand();
        var user = UserFactory.CreateUser();

        _mockUserRepository
            .Setup(u => u.GetUserByIdAsync(UserId.Create(command.UserId)))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.IsType<Success>(result.Value);

        _mockUserRepository.Verify(
            x => x.GetUserByIdAsync(UserId.Create(command.UserId)),
            Times.Once
        );
        _mockUserRepository.Verify(x => x.Update(user), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnUserNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = UpdateUserCommandUtils.CreateCommand();

        _mockUserRepository
            .Setup(u => u.GetUserByIdAsync(UserId.Create(command.UserId)))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(CustomErrors.User.UserNotFound, result.Errors[0]);

        _mockUserRepository.Verify(
            x => x.GetUserByIdAsync(UserId.Create(command.UserId)),
            Times.Once
        );
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void UpdateUserCommand_Should_DeclareUserCacheKeyForInvalidation()
    {
        // Arrange
        var command = UpdateUserCommandUtils.CreateCommand();

        // Assert
        Assert.IsAssignableFrom<IInvalidateCache>(command);
        Assert.Contains(CacheKeys.User(command.UserId), command.CacheKeysToInvalidate);
    }
}
