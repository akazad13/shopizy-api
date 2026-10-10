using FluentValidation;

namespace Shopizy.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("Category ID must not be empty.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
