using FluentValidation;

namespace Shopizy.Application.Carts.Commands.AddProductToCart;

public class AddProductToCartCommandValidator : AbstractValidator<AddProductToCartCommand>
{
    public AddProductToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("User ID must not be empty.");
        RuleFor(x => x.ProductId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("Product ID must not be empty.");
        RuleFor(x => x.Color).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Size).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
