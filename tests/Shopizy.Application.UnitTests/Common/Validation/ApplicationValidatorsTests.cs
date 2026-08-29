using FluentValidation.TestHelper;
using Shopizy.Application.Products.Commands.BulkUpdateProductStatus;
using Shopizy.Application.Products.Commands.DeleteProductImage;
using Shopizy.Application.Products.Commands.RemoveVariant;
using Shopizy.Application.Products.Commands.UpdateVariant;
using Shopizy.Application.PromoCodes.Commands.CreatePromoCode;
using Shopizy.Application.PromoCodes.Commands.UpdatePromoCode;
using Shopizy.Application.PromoCodes.Queries.GetPromoCodes;
using Shopizy.Application.Users.Commands.AddUserAddress;
using Shopizy.Application.Users.Commands.DeleteUserAddress;
using Shopizy.Application.Users.Commands.ForgotPassword;
using Shopizy.Application.Users.Commands.ResetPassword;
using Shopizy.Application.Users.Commands.SetDefaultAddress;
using Shopizy.Application.Users.Commands.UpdateUserAddress;
using Shouldly;
using Xunit;

namespace Shopizy.Application.UnitTests.Common.Validation;

public class ApplicationValidatorsTests
{
    [Fact]
    public void AddUserAddressCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new AddUserAddressCommandValidator();
        var command = new AddUserAddressCommand(
            Guid.NewGuid(),
            "Street",
            "City",
            "State",
            "Country",
            "12345",
            true
        );

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteUserAddressCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new DeleteUserAddressCommandValidator();
        var command = new DeleteUserAddressCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ForgotPasswordCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new ForgotPasswordCommandValidator();
        var command = new ForgotPasswordCommand("test@example.com");

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ResetPasswordCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new ResetPasswordCommandValidator();
        var command = new ResetPasswordCommand("token123", "ValidP@ss123");

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SetDefaultAddressCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new SetDefaultAddressCommandValidator();
        var command = new SetDefaultAddressCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateUserAddressCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new UpdateUserAddressCommandValidator();
        var command = new UpdateUserAddressCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Street",
            "City",
            "State",
            "Country",
            "12345"
        );

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePromoCodeCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new CreatePromoCodeCommandValidator();
        var command = new CreatePromoCodeCommand("PROMO10", "10 Off", 10m, true, true);

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePromoCodeCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new UpdatePromoCodeCommandValidator();
        var command = new UpdatePromoCodeCommand(
            Guid.NewGuid(),
            "PROMO20",
            "20 Off",
            20m,
            false,
            true
        );

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetPromoCodesQueryValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new GetPromoCodesQueryValidator();
        var query = new GetPromoCodesQuery(1, 10);

        var result = validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void BulkUpdateProductStatusCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new BulkUpdateProductStatusCommandValidator();
        var command = new BulkUpdateProductStatusCommand([Guid.NewGuid()], true);

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteProductImageCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new DeleteProductImageCommandValidator();
        var command = new DeleteProductImageCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RemoveVariantCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new RemoveVariantCommandValidator();
        var command = new RemoveVariantCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateVariantCommandValidator_WhenValid_ShouldNotHaveErrors()
    {
        var validator = new UpdateVariantCommandValidator();
        var command = new UpdateVariantCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Var1",
            "SKU1",
            10m,
            Shopizy.Domain.Common.Enums.Currency.usd,
            5,
            true
        );

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
