using Shopizy.Domain.Products;

namespace Shopizy.Application.Common.Interfaces.Persistence;

/// <summary>
/// Read-only product queries that don't load full aggregates.
/// Pulled out of <see cref="IProductRepository"/> as the first slice of the A2 repository slim-down:
/// query handlers should depend on this rather than the full repository.
/// </summary>
public interface IProductReader
{
    /// <summary>
    /// Returns the total number of products. Used by the admin dashboard metrics.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns up to 10 products whose stock has dropped to or below <paramref name="threshold"/>,
    /// ordered by stock ascending. Used by the low-stock dashboard alert.
    /// </summary>
    /// <param name="threshold"></param>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<Product>> GetLowStockAsync(
        int threshold,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the distinct brand names referenced by products, suitable for filter UIs.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<string>> GetBrandNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches and pages products matching the specified criteria, returning the matching products and total match count.
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cancellationToken"></param>
    Task<(IReadOnlyList<Product> Products, int TotalCount)> GetProductsWithCountAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default
    );
}
