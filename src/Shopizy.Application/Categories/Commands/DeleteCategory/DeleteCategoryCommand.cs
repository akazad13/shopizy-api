using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Represents a command to delete a category.
/// </summary>
/// <param name="UserId">The ID of the admin user performing the action.</param>
/// <param name="CategoryId">The category's unique identifier to delete.</param>
public record DeleteCategoryCommand(Guid UserId, Guid CategoryId)
    : ICommand<ErrorOr<Success>>,
        IInvalidateCache
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate =>
        ["categories-tree", "categories-all", $"category:{CategoryId}"];
}
