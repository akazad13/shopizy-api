using FluentValidation.TestHelper;
using Shopizy.Application.Products.Commands.CreateProduct;
using Shopizy.Application.Products.Commands.DeleteProduct;
using Shopizy.Application.Products.Commands.UpdateProduct;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Common.ValueObjects;

namespace Shopizy.Application.UnitTests.Products.Commands;

public class ProductCommandValidatorsTests
{
    [Fact]
    public void CreateProductCommandValidator_ShouldValidateFields()
    {
        var validator = new CreateProductCommandValidator();

        var validCommand = new CreateProductCommand(
            Guid.NewGuid(),
            "T-Shirt",
            "Short desc",
            "Long desc",
            CategoryId.Create(Guid.NewGuid()),
            Price.CreateNew(29.99m, Currency.usd),
            0m,
            "TSHIRT-001",
            10,
            null,
            "Red,Blue",
            "S,M,L",
            "Casual,Cotton",
            "123456",
            null
        );

        validator.TestValidate(validCommand).ShouldNotHaveAnyValidationErrors();

        var invalidCommand = new CreateProductCommand(
            Guid.Empty,
            "",
            new string('a', 101),
            new string('b', 2001),
            CategoryId.Create(Guid.NewGuid()),
            Price.CreateNew(0, Currency.usd),
            -1m,
            "",
            -1,
            null,
            "",
            "",
            new string('t', 201),
            new string('c', 51),
            null
        );

        var result = validator.TestValidate(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.ShouldHaveValidationErrorFor(c => c.ShortDescription);
        result.ShouldHaveValidationErrorFor(c => c.Description);
        result.ShouldHaveValidationErrorFor(c => c.Sku);
        result.ShouldHaveValidationErrorFor(c => c.UnitPrice);
        result.ShouldHaveValidationErrorFor(c => c.StockQuantity);
        result.ShouldHaveValidationErrorFor(c => c.Barcode);
        result.ShouldHaveValidationErrorFor(c => c.Colors);
        result.ShouldHaveValidationErrorFor(c => c.Sizes);
        result.ShouldHaveValidationErrorFor(c => c.Tags);
    }

    [Fact]
    public void UpdateProductCommandValidator_ShouldValidateFields()
    {
        var validator = new UpdateProductCommandValidator();

        var validCommand = new UpdateProductCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Updated Shirt",
            "Short desc",
            "Long desc",
            CategoryId.Create(Guid.NewGuid()),
            Price.CreateNew(35.00m, Currency.usd),
            5.00m,
            "TSHIRT-001-UPD",
            null,
            "Green",
            "XL",
            "Summer",
            "123456789",
            20,
            null
        );

        validator.TestValidate(validCommand).ShouldNotHaveAnyValidationErrors();

        var invalidCommand = new UpdateProductCommand(
            Guid.Empty,
            Guid.Empty,
            "",
            new string('a', 101),
            new string('b', 2001),
            CategoryId.Create(Guid.Empty),
            Price.CreateNew(0m, Currency.usd),
            -1m,
            "",
            null,
            "",
            "",
            new string('t', 201),
            new string('c', 51),
            -1,
            null
        );

        var result = validator.TestValidate(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.ProductId);
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
        result.ShouldHaveValidationErrorFor(c => c.Sku);
        result.ShouldHaveValidationErrorFor(c => c.UnitPrice);
        result.ShouldHaveValidationErrorFor(c => c.Discount);
        result.ShouldHaveValidationErrorFor(c => c.Colors);
        result.ShouldHaveValidationErrorFor(c => c.Sizes);
    }

    [Fact]
    public void DeleteProductCommandValidator_ShouldValidateFields()
    {
        var validator = new DeleteProductCommandValidator();

        var validCommand = new DeleteProductCommand(Guid.NewGuid(), Guid.NewGuid());
        validator.TestValidate(validCommand).ShouldNotHaveAnyValidationErrors();

        var invalidCommand = new DeleteProductCommand(Guid.Empty, Guid.Empty);
        var result = validator.TestValidate(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.UserId);
        result.ShouldHaveValidationErrorFor(c => c.ProductId);
    }
}
