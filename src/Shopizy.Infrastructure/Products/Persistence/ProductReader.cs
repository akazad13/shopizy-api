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
        _dbContext.Products.AsNoTracking().CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(
        int threshold,
        CancellationToken cancellationToken = default
    ) =>
        await _dbContext
            .Products.AsNoTracking()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
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
}
