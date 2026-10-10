using FluentValidation;

namespace Shopizy.Application.Carts.Commands.UpdateProductQuantity;

public class UpdateProductQuantityCommandValidator : AbstractValidator<UpdateProductQuantityCommand>
{
    public UpdateProductQuantityCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("User ID must not be empty.");
        RuleFor(x => x.CartItemId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("Cart item ID must not be empty.");
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
