using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Products.Persistence;

/// <summary>
/// Repository for managing product data persistence.
/// </summary>
/// <param name="dbContext"></param>
public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Retrieves a product by its unique identifier, including reviews.
    /// </summary>
    /// <param name="id"></param>
    public Task<Product?> GetProductByIdAsync(ProductId id) =>
        _dbContext
            .Products.Include(p => p.ProductImages)
            .Include(p => p.ProductReviews)
                .ThenInclude(p => p.User)
            .AsNoTracking()
            .Cacheable()
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetProductByIdForUpdateAsync(ProductId id) =>
        _dbContext.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);

    /// <summary>
    /// Retrieves multiple products by their identifiers.
    /// </summary>
    /// <param name="ids"></param>
    public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IReadOnlyList<ProductId> ids) =>
        await _dbContext.Products.Where(p => ids.Contains(p.Id)).AsNoTracking().ToListAsync();

    public async Task<IReadOnlyList<Product>> GetProductsByIdsForUpdateAsync(
        IReadOnlyList<ProductId> ids
    ) => await _dbContext.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

    public Task<bool> IsProductExistAsync(ProductId id) =>
        _dbContext.Products.AnyAsync(p => p.Id == id);

    public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetProductsWithCountAsync(
        ProductSearchCriteria criteria
    )
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = BuildProductCriteriaQuery(criteria);
        var totalCount = await query.CountAsync();

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
            .ToListAsync();

        return (products, totalCount);
    }

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    /// <param name="product"></param>
    public async Task AddAsync(Product product) => await _dbContext.Products.AddAsync(product);

    /// <summary>
    /// Updates an existing product in the database.
    /// </summary>
    /// <param name="product"></param>
    public void Update(Product product) => _dbContext.Update(product);

    /// <summary>
    /// Removes a product from the database.
    /// </summary>
    /// <param name="product"></param>
    public void Remove(Product product) => _dbContext.Remove(product);

    /// <summary>
    /// Removes multiple products from the database.
    /// </summary>
    /// <param name="products"></param>
    public void RemoveRange(IList<Product> products) => _dbContext.RemoveRange(products);

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
