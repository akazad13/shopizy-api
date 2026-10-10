using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Queries.ListCategories;

/// <summary>
/// Represents a query to retrieve all categories.
/// </summary>
public record ListCategoriesQuery() : IQuery<ErrorOr<List<CategoryItem>>>, ICachableRequest
{
    public string CacheKey => CacheKeys.CategoriesAll;
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
