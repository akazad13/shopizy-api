using ErrorOr;
using Shopizy.Domain.Categories;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Represents a command to create a new category.
/// </summary>
/// <param name="UserId">The ID of the admin user performing the action.</param>
/// <param name="Name">The category name.</param>
/// <param name="ParentId">The parent category ID (null for root categories).</param>
public record CreateCategoryCommand(Guid UserId, string Name, Guid? ParentId)
    : ICommand<ErrorOr<Category>>,
        IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        ["categories-tree", "categories-all"];
}
