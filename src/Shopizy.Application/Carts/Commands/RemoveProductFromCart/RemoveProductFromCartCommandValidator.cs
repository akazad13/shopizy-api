using FluentValidation;

namespace Shopizy.Application.Carts.Commands.RemoveProductFromCart;

public class RemoveProductFromCartCommandValidator : AbstractValidator<RemoveProductFromCartCommand>
{
    public RemoveProductFromCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("User ID must not be empty.");
        RuleFor(x => x.ItemId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("Cart item ID must not be empty.");
    }
}
