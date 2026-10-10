using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Permissions;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Permissions.Persistence;

public class PermissionRepository(AppDbContext dbContext) : IPermissionRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<Permission>> GetAsync() =>
        await _dbContext.Permissions.AsNoTracking().Cacheable().ToListAsync();

    public Task<Permission?> GetByIdAsync(PermissionId id) =>
        _dbContext.Permissions.SingleOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Permission user) => await _dbContext.Permissions.AddAsync(user);

    public void Update(Permission permission) => _dbContext.Permissions.Update(permission);
}
