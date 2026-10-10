using Mapster;
using Shopizy.Application.Categories.Queries.CategoriesTree;
using Shopizy.Application.Categories.Queries.GetCategory;
using Shopizy.Application.Categories.Queries.ListCategories;
using Shopizy.Contracts.Category;
using Shopizy.Domain.Categories;

namespace Shopizy.Api.Common.Mapping;

/// <summary>
/// Configures mapping for category-related models.
/// </summary>
public class CategoryMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configurations.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

#pragma warning disable CS8625
        config
            .NewConfig<Category, CategoryResponse>()
            .Map(dest => dest.Id, src => (object?)src.Id != null ? src.Id.Value : Guid.Empty);

        config
            .NewConfig<Category, CategoryTreeResponse>()
            .Map(dest => dest.Id, src => (object?)src.Id != null ? src.Id.Value : Guid.Empty);
#pragma warning restore CS8625

        config.NewConfig<CategoryTree, CategoryTreeResponse>();

        config.NewConfig<CategoryItem, CategoryResponse>();

        config.NewConfig<Guid, GetCategoryQuery>().MapWith(src => new GetCategoryQuery(src));

        config
            .NewConfig<
                (Guid UserId, Guid CategoryId, UpdateCategoryRequest request),
                Shopizy.Application.Categories.Commands.UpdateCategory.UpdateCategoryCommand
            >()
            .MapWith(
                src => new Shopizy.Application.Categories.Commands.UpdateCategory.UpdateCategoryCommand(
                    src.UserId,
                    Shopizy.Domain.Categories.ValueObjects.CategoryId.Create(src.CategoryId),
                    src.request.Name,
                    src.request.ParentId
                )
            );

        config
            .NewConfig<
                (Guid UserId, Guid CategoryId),
                Shopizy.Application.Categories.Commands.DeleteCategory.DeleteCategoryCommand
            >()
            .MapWith(
                src => new Shopizy.Application.Categories.Commands.DeleteCategory.DeleteCategoryCommand(
                    src.UserId,
                    Shopizy.Domain.Categories.ValueObjects.CategoryId.Create(src.CategoryId)
                )
            );
    }
}
