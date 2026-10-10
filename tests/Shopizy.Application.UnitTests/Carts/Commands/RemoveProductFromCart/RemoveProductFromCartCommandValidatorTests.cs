using FluentValidation.TestHelper;
using Shopizy.Application.Carts.Commands.RemoveProductFromCart;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Carts.Commands.RemoveProductFromCart;

public class RemoveProductFromCartCommandValidatorTests
{
    private readonly RemoveProductFromCartCommandValidator _validator = new();

    [Fact]
    public async Task Should_HaveError_When_UserIdIsEmpty()
    {
        var command = new RemoveProductFromCartCommand(
            UserId.Create(Guid.Empty),
            CartItemId.CreateUnique()
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Should_HaveError_When_UserIdIsNull()
    {
        var command = new RemoveProductFromCartCommand(null!, CartItemId.CreateUnique());
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Should_HaveError_When_ItemIdIsEmpty()
    {
        var command = new RemoveProductFromCartCommand(
            UserId.CreateUnique(),
            CartItemId.Create(Guid.Empty)
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public async Task Should_HaveError_When_ItemIdIsNull()
    {
        var command = new RemoveProductFromCartCommand(UserId.CreateUnique(), null!);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public async Task Should_NotHaveErrors_When_AllFieldsAreValid()
    {
        var command = new RemoveProductFromCartCommand(
            UserId.CreateUnique(),
            CartItemId.CreateUnique()
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldNotHaveAnyValidationErrors();
    }
}
