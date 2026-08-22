using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Queries.CategoriesTree;

public class CategoriesTreeQueryHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<CategoriesTreeQuery, ErrorOr<List<CategoryTree>>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<List<CategoryTree>>> Handle(
        CategoriesTreeQuery query,
        CancellationToken cancellationToken
    )
    {
        var categories = await _categoryRepository.GetCategoriesAsync();

        var allCategoryNodes = categories
            .Select(category => new CategoryTree()
            {
                Id = category.Id.Value,
                Name = category.Name,
                ParentId = category.ParentId,
            })
            .ToList();

        var categoriesLookup = allCategoryNodes.ToLookup(c => c.ParentId);

        return BuildCategoryTree(categoriesLookup);
    }

    private static List<CategoryTree> BuildCategoryTree(
        ILookup<Guid?, CategoryTree> categoriesLookup,
        Guid? parentId = null
    )
    {
        var subCategories = categoriesLookup[parentId].ToList();

        foreach (var category in subCategories)
        {
            category.Children = BuildCategoryTree(categoriesLookup, category.Id);
        }

        return subCategories;
    }
}
