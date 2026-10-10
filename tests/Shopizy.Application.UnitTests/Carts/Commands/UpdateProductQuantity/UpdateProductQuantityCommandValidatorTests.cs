using FluentValidation.TestHelper;
using Shopizy.Application.Carts.Commands.UpdateProductQuantity;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Carts.Commands.UpdateProductQuantity;

public class UpdateProductQuantityCommandValidatorTests
{
    private readonly UpdateProductQuantityCommandValidator _validator = new();

    [Fact]
    public async Task Should_HaveError_When_UserIdIsEmpty()
    {
        var command = new UpdateProductQuantityCommand(
            UserId.Create(Guid.Empty),
            CartItemId.CreateUnique(),
            1
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
        var command = new UpdateProductQuantityCommand(null!, CartItemId.CreateUnique(), 1);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Should_HaveError_When_CartItemIdIsEmpty()
    {
        var command = new UpdateProductQuantityCommand(
            UserId.CreateUnique(),
            CartItemId.Create(Guid.Empty),
            1
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.CartItemId);
    }

    [Fact]
    public async Task Should_HaveError_When_CartItemIdIsNull()
    {
        var command = new UpdateProductQuantityCommand(UserId.CreateUnique(), null!, 1);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.CartItemId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Should_HaveError_When_QuantityIsNotPositive(int quantity)
    {
        var command = new UpdateProductQuantityCommand(
            UserId.CreateUnique(),
            CartItemId.CreateUnique(),
            quantity
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public async Task Should_NotHaveErrors_When_AllFieldsAreValid()
    {
        var command = new UpdateProductQuantityCommand(
            UserId.CreateUnique(),
            CartItemId.CreateUnique(),
            3
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldNotHaveAnyValidationErrors();
    }
}
