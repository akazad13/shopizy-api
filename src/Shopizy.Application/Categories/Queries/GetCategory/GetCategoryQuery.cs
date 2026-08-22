using ErrorOr;
using Shopizy.Domain.Categories;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Categories.Queries.GetCategory;

/// <summary>
/// Represents a query to retrieve a category by its ID.
/// </summary>
/// <param name="CategoryId">The category's unique identifier.</param>
public record GetCategoryQuery(Guid CategoryId) : IQuery<ErrorOr<Category>>, ICachableRequest
{
    public string CacheKey => $"category:{CategoryId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
