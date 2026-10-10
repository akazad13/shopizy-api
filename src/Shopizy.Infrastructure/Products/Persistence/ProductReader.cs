using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Products;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Products.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IProductReader"/>. All queries are <see cref="EntityFrameworkQueryableExtensions.AsNoTracking"/>.
/// </summary>
/// <param name="dbContext"></param>
public sealed class ProductReader(AppDbContext dbContext) : IProductReader
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Products.AsNoTracking().Cacheable().CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(
        int threshold,
        CancellationToken cancellationToken = default
    ) =>
        await _dbContext
            .Products.AsNoTracking()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Cacheable()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetBrandNamesAsync(
        CancellationToken cancellationToken = default
    ) =>
        await _dbContext
            .Brands.AsNoTracking()
            .Select(b => b.Name)
            .Distinct()
            .Cacheable()
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetProductsWithCountAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = BuildProductCriteriaQuery(criteria);
        var totalCount = await query.CountAsync(cancellationToken);

        var sortedQuery = criteria.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.UnitPrice.Amount),
            "price_desc" => query.OrderByDescending(p => p.UnitPrice.Amount),
            "newest" => query.OrderByDescending(p => p.CreatedOn),
            "best_rated" => query.OrderByDescending(p => p.AverageRating.Value),
            "most_reviewed" => query.OrderByDescending(p => p.AverageRating.NumRatings),
            _ => query.OrderByDescending(p => p.CreatedOn),
        };

        var products = await sortedQuery
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    private IQueryable<Product> BuildProductCriteriaQuery(ProductSearchCriteria criteria)
    {
        var query = _dbContext.Products.Include(p => p.ProductImages).AsQueryable();

        if (criteria.ProductIds is not null)
        {
            query = query.Where(p => criteria.ProductIds.Contains(p.Id));
        }

        if (criteria.Name is not null)
        {
            query = query.Where(p => p.Name.Contains(criteria.Name));
        }

        if (criteria.CategoryIds is not null)
        {
            query = query.Where(p => criteria.CategoryIds.Contains(p.CategoryId));
        }

        if (criteria.BrandIds is not null)
        {
            query = query.Where(p => criteria.BrandIds.Contains(p.BrandId));
        }

        if (criteria.AverageRating is not null)
        {
            query = query.Where(p => p.AverageRating.Value >= criteria.AverageRating);
        }

        if (criteria.MinPrice is not null)
        {
            query = query.Where(p => p.UnitPrice.Amount >= criteria.MinPrice);
        }

        if (criteria.MaxPrice is not null)
        {
            query = query.Where(p => p.UnitPrice.Amount <= criteria.MaxPrice);
        }

        if (criteria.InStockOnly is true)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        return query;
    }
}
