using FluentValidation;

namespace Shopizy.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Validator for the <see cref="UpdateProductCommand"/>.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);

        RuleFor(x => x.ShortDescription).MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Highlights).MaximumLength(1000);

        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);

        RuleFor(x => x.UnitPrice).GreaterThan(0);

        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Barcode).MaximumLength(50);

        RuleFor(x => x.Colors).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Sizes).NotEmpty().MaximumLength(20);

        RuleFor(x => x.Tags).MaximumLength(200);
    }
}
