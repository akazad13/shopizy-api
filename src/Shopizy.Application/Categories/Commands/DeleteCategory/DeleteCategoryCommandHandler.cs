using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    : ICommandHandler<DeleteCategoryCommand, ErrorOr<Success>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<Success>> Handle(
        DeleteCategoryCommand cmd,
        CancellationToken cancellationToken = default
    )
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(cmd.CategoryId);
        if (category is null)
        {
            return (Error)CustomErrors.Category.CategoryNotFound;
        }

        _categoryRepository.Remove(category);

        return Result.Success;
    }
}
