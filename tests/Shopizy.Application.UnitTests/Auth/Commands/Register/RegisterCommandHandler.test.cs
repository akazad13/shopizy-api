using ErrorOr;
using Moq;
using Shopizy.Application.Auth.Commands.Register;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Enums;

namespace Shopizy.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private static readonly IReadOnlyList<PermissionId> SeededPermissionIds =
    [
        PermissionId.Create(new("249E733D-5BDC-49C3-91CA-06AE25A9C897")), // create:cart
        PermissionId.Create(new("2A19090A-B3F3-4B30-9CED-934EE0503D26")), // create:order
        PermissionId.Create(new("D6C2E3C6-314B-4F2E-A407-34139B145771")), // delete:cart
        PermissionId.Create(new("DD25381D-063C-4A3A-9539-DEEC640919A4")), // delete:order
        PermissionId.Create(new("4B88CB16-0228-4669-BA7F-B75F42A3B7AF")), // get:cart
        PermissionId.Create(new("9601BA5E-EB54-4487-BFE0-563462D3CC25")), // get:order
        PermissionId.Create(new("5E2A486B-D9A0-4F83-8FF2-C56EF97CE485")), // get:category
        PermissionId.Create(new("0C65A58A-D472-4D5D-848E-EAC46F988F5D")), // get:product
        PermissionId.Create(new("0374E597-604E-4146-8F40-8C994D26C290")), // get:user
        PermissionId.Create(new("20082930-3857-4B34-80D0-E256B9B585D8")), // modify:cart
        PermissionId.Create(new("ACD9D507-AC45-4CD2-B0F4-91126C71319A")), // modify:order
        PermissionId.Create(new("C920A577-1669-4167-B056-5E0A03329C55")), // modify:user
        PermissionId.Create(new("9b259d3d-b634-4232-9deb-e5fdb20d7a64")), // get:wishlist
        PermissionId.Create(new("759b8d6d-ffda-4c99-bf29-ed335c029a5c")), // modify:wishlist
        PermissionId.Create(new("d99cab25-5af2-4b9c-9fad-385e4715d7f2")), // create:wishlist
    ];

    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordManager> _mockPasswordManager;
    private readonly Mock<IPermissionLookup> _mockPermissionLookup;
    private readonly Mock<Shopizy.SharedKernel.Application.Interfaces.Persistence.IUnitOfWork> _mockUnitOfWork;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordManager = new Mock<IPasswordManager>();
        _mockPermissionLookup = new Mock<IPermissionLookup>();
        _mockUnitOfWork =
            new Mock<Shopizy.SharedKernel.Application.Interfaces.Persistence.IUnitOfWork>();
        _mockPermissionLookup
            .Setup(p =>
                p.GetIdsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(SeededPermissionIds);
        _handler = new RegisterCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordManager.Object,
            _mockPermissionLookup.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Should_ReturnDuplicateEmailError_WhenUserWithSameEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand("John", "Doe", "test@test.com", "password123");

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(command.Email))
            .ReturnsAsync(
                User.Create(
                    "Existing",
                    "User",
                    command.Email,
                    "hashedPassword",
                    UserRole.Customer,
                    []
                )
            );

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(CustomErrors.User.DuplicateEmail, result.Errors);
        _mockUserRepository.Verify(r => r.GetUserByEmailAsync(command.Email), Times.Once);
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Should_CreateNewUserWithHashedPassword_WhenValidInputIsProvided()
    {
        // Arrange
        var command = new RegisterCommand("John", "Doe", "test@test.com", "password123");
        var hashedPassword = "hashedPassword123";
        var expectedPermissionIds = SeededPermissionIds;

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockPasswordManager
            .Setup(pm => pm.CreateHashString(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        _mockUserRepository.Verify(
            r =>
                r.AddAsync(
                    It.Is<User>(u =>
                        u.FirstName == command.FirstName
                        && u.LastName == command.LastName
                        && u.Email == command.Email
                        && u.Password == hashedPassword
                        && u.Role == UserRole.Customer
                        && u.PermissionIds.Select(p => p.Value)
                            .OrderBy(v => v)
                            .SequenceEqual(
                                expectedPermissionIds.Select(p => p.Value).OrderBy(v => v)
                            )
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_AssignAllDefaultPermissions_WhenCreatingNewUser()
    {
        // Arrange
        var command = new RegisterCommand("John", "Doe", "test@gmail.com", "password123");
        var hashedPassword = "hashedPassword123";

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _mockPasswordManager
            .Setup(pm => pm.CreateHashString(command.Password, It.IsAny<int>()))
            .Returns(hashedPassword);

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : "");
        Assert.IsType<Success>(result.Value);

        _mockUserRepository.Verify(
            r =>
                r.AddAsync(
                    It.Is<User>(u => u.PermissionIds.Count == 15 && u.Role == UserRole.Customer)
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("  ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    [InlineData("", "")]
    [InlineData("  ", "   ")]
    public async Task Should_ReturnValidationErrorWhen_FirstNameOrLastNameIsEmptyOrWhitespace(
        string firstName,
        string lastName
    )
    {
        // Arrange
        var command = new RegisterCommand(firstName, lastName, "test@gmail.com", "password123");

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(CustomErrors.User.InvalidName, result.Errors);
    }

    [Fact]
    public async Task Should_EnsurePasswordIsHashed_BeforeStoringInDatabase()
    {
        // Arrange
        var command = new RegisterCommand("John", "Doe", "test@gmail.com", "password123");
        var hashedPassword = "hashedPassword123";

        _mockUserRepository
            .Setup(r => r.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);
        _mockPasswordManager
            .Setup(pm => pm.CreateHashString(command.Password, It.IsAny<int>()))
            .Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.IsType<Success>(result.Value);
        _mockPasswordManager.Verify(
            pm => pm.CreateHashString(command.Password, It.IsAny<int>()),
            Times.Once
        );
        _mockUserRepository.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Password == hashedPassword)),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_HandleConcurrentRegistrationAttempts_ForSameEmail()
    {
        // Arrange
        var command = new RegisterCommand("John", "Doe", "test@test.com", "password123");
        var hashedPassword = "hashedPassword123";

        _mockUserRepository
            .SetupSequence(r => r.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((User?)null)
            .ReturnsAsync(
                User.Create(
                    "Existing",
                    "User",
                    command.Email,
                    "existingHash",
                    UserRole.Customer,
                    []
                )
            );

        _mockPasswordManager
            .Setup(pm => pm.CreateHashString(command.Password, It.IsAny<int>()))
            .Returns(hashedPassword);

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var task1 = _handler.Handle(command, TestContext.Current.CancellationToken);
        var task2 = _handler.Handle(command, TestContext.Current.CancellationToken);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Equal(2, results.Length);
        Assert.False(results[0].IsError);
        Assert.IsType<Success>(results[0].Value);
        Assert.True(results[1].IsError);
        Assert.Contains(CustomErrors.User.DuplicateEmail, results[1].Errors);
    }
}
