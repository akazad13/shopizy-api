using FluentValidation.TestHelper;
using Shopizy.Application.Users.Commands.UpdateUserRole;

namespace Shopizy.Application.UnitTests.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidatorTests
{
    private readonly UpdateUserRoleCommandValidator _validator = new();

    [Fact]
    public async Task Should_HaveError_When_UserIdIsEmpty()
    {
        var command = new UpdateUserRoleCommand(Guid.Empty, "Admin", [], Guid.NewGuid());
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Should_HaveError_When_ModifiedByIdIsEmpty()
    {
        var command = new UpdateUserRoleCommand(Guid.NewGuid(), "Admin", [], Guid.Empty);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.ModifiedById);
    }

    [Fact]
    public async Task Should_HaveError_When_RoleIsEmpty()
    {
        var command = new UpdateUserRoleCommand(Guid.NewGuid(), "", [], Guid.NewGuid());
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public async Task Should_NotHaveError_When_PermissionIdsIsNull()
    {
        var command = new UpdateUserRoleCommand(Guid.NewGuid(), "Admin", null, Guid.NewGuid());
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldNotHaveValidationErrorFor(x => x.PermissionIds);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllFieldsAreValid()
    {
        var command = new UpdateUserRoleCommand(
            Guid.NewGuid(),
            "Admin",
            [Guid.NewGuid()],
            Guid.NewGuid()
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldNotHaveAnyValidationErrors();
    }
}
