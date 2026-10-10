using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Brands;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Brands.Persistence;

public class BrandRepository(AppDbContext dbContext) : IBrandRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<Brand?> GetByIdAsync(BrandId id) =>
        _dbContext.Brands.FirstOrDefaultAsync(brand => brand.Id == id);

    public Task<Brand?> GetByNameAsync(string name) =>
        _dbContext.Brands.FirstOrDefaultAsync(brand => brand.Name == name);

    public async Task<IReadOnlyList<Brand>> GetAsync() =>
        await _dbContext
            .Brands.AsNoTracking()
            .OrderBy(brand => brand.Name)
            .Cacheable()
            .ToListAsync();

    public async Task AddAsync(Brand brand) => await _dbContext.Brands.AddAsync(brand);

    public void Remove(Brand brand) => _dbContext.Remove(brand);
}
