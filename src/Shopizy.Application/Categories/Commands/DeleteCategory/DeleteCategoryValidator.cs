using FluentValidation;

namespace Shopizy.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotNull()
            .Must(x => x is not null && x.Value != Guid.Empty)
            .WithMessage("Category ID must not be empty.");
    }
}
