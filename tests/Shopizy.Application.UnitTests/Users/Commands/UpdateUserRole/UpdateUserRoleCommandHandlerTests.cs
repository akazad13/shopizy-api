using ErrorOr;
using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Security.Permissions;
using Shopizy.Application.UnitTests.Users.TestUtils;
using Shopizy.Application.Users.Commands.UpdateUserRole;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Interfaces.Persistence;
using Shouldly;

namespace Shopizy.Application.UnitTests.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPermissionLookup> _mockPermissionLookup;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPermissionLookup = new Mock<IPermissionLookup>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new UpdateUserRoleCommandHandler(
            _mockUserRepository.Object,
            _mockPermissionLookup.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var command = new UpdateUserRoleCommand(
            Guid.NewGuid(),
            "Admin",
            [Guid.NewGuid()],
            Guid.NewGuid()
        );

        _mockUserRepository
            .Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>()))
            .ReturnsAsync((Shopizy.Domain.Users.User?)null);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(CustomErrors.User.UserNotFound);
    }

    [Fact]
    public async Task Handle_WhenValidRoleAndPermissions_ShouldUpdateUserRoleAndPermissions()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var permGuid = Guid.NewGuid();
        var command = new UpdateUserRoleCommand(user.Id.Value, "Admin", [permGuid], Guid.NewGuid());

        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);
        user.Role.ShouldBe(UserRole.Admin);
        user.PermissionIds.Count.ShouldBe(1);
        user.PermissionIds[0].Value.ShouldBe(permGuid);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleInvalid_ShouldNotUpdateRoleButUpdatePermissions()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var initialRole = user.Role;
        var command = new UpdateUserRoleCommand(
            user.Id.Value,
            "InvalidRoleName",
            [],
            Guid.NewGuid()
        );

        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        user.Role.ShouldBe(initialRole);
        user.PermissionIds.Count.ShouldBe(0);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPermissionIdsIsNull_AndRoleIsAdmin_ShouldAssignAllPermissions()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var adminPermIds = new List<PermissionId>
        {
            PermissionId.Create(Guid.NewGuid()),
            PermissionId.Create(Guid.NewGuid()),
            PermissionId.Create(Guid.NewGuid()),
        };
        var command = new UpdateUserRoleCommand(user.Id.Value, "Admin", null, Guid.NewGuid());

        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        _mockPermissionLookup
            .Setup(p => p.GetAllIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminPermIds);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        user.Role.ShouldBe(UserRole.Admin);
        user.PermissionIds.Count.ShouldBe(adminPermIds.Count);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPermissionIdsIsNull_AndRoleIsCustomer_ShouldAssignCustomerPermissions()
    {
        // Arrange
        var user = UserFactory.CreateUser();
        var customerPermIds = new List<PermissionId>
        {
            PermissionId.Create(Guid.NewGuid()),
            PermissionId.Create(Guid.NewGuid()),
        };
        var command = new UpdateUserRoleCommand(user.Id.Value, "Customer", null, Guid.NewGuid());

        _mockUserRepository.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
        _mockPermissionLookup
            .Setup(p =>
                p.GetIdsByNamesAsync(RolePermissions.Customer, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(customerPermIds);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        user.Role.ShouldBe(UserRole.Customer);
        user.PermissionIds.Count.ShouldBe(customerPermIds.Count);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
