using FluentValidation.TestHelper;
using Shopizy.Application.Carts.Commands.AddProductToCart;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Carts.Commands.AddProductToCart;

public class AddProductToCartCommandValidatorTests
{
    private readonly AddProductToCartCommandValidator _validator = new();

    [Fact]
    public async Task Should_HaveError_When_UserIdIsEmpty()
    {
        var command = new AddProductToCartCommand(
            UserId.Create(Guid.Empty),
            ProductId.CreateUnique(),
            "Red",
            "M",
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
        var command = new AddProductToCartCommand(null!, ProductId.CreateUnique(), "Red", "M", 1);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Should_HaveError_When_ProductIdIsEmpty()
    {
        var command = new AddProductToCartCommand(
            UserId.CreateUnique(),
            ProductId.Create(Guid.Empty),
            "Red",
            "M",
            1
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public async Task Should_HaveError_When_ProductIdIsNull()
    {
        var command = new AddProductToCartCommand(UserId.CreateUnique(), null!, "Red", "M", 1);
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public async Task Should_HaveError_When_ColorIsEmpty()
    {
        var command = new AddProductToCartCommand(
            UserId.CreateUnique(),
            ProductId.CreateUnique(),
            "",
            "M",
            1
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.Color);
    }

    [Fact]
    public async Task Should_HaveError_When_SizeIsEmpty()
    {
        var command = new AddProductToCartCommand(
            UserId.CreateUnique(),
            ProductId.CreateUnique(),
            "Red",
            "",
            1
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_HaveError_When_QuantityIsNotPositive(int quantity)
    {
        var command = new AddProductToCartCommand(
            UserId.CreateUnique(),
            ProductId.CreateUnique(),
            "Red",
            "M",
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
        var command = new AddProductToCartCommand(
            UserId.CreateUnique(),
            ProductId.CreateUnique(),
            "Red",
            "M",
            2
        );
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.ShouldNotHaveAnyValidationErrors();
    }
}
