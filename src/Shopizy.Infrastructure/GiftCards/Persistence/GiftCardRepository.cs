using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.GiftCards;
using Shopizy.Domain.GiftCards.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.GiftCards.Persistence;

public class GiftCardRepository(AppDbContext dbContext) : IGiftCardRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<GiftCard>> GetAllAsync(int pageNumber, int pageSize) =>
        await _dbContext
            .Set<GiftCard>()
            .AsNoTracking()
            .OrderByDescending(gc => gc.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Cacheable()
            .ToListAsync();

    public Task<GiftCard?> GetByIdAsync(GiftCardId id) =>
        _dbContext.Set<GiftCard>().FirstOrDefaultAsync(gc => gc.Id == id);

    public Task<GiftCard?> GetByCodeAsync(string code) =>
        _dbContext.Set<GiftCard>().FirstOrDefaultAsync(gc => gc.Code == code);

    public async Task AddAsync(GiftCard giftCard) =>
        await _dbContext.Set<GiftCard>().AddAsync(giftCard);

    public void Update(GiftCard giftCard) => _dbContext.Set<GiftCard>().Update(giftCard);

    public void Remove(GiftCard giftCard) => _dbContext.Set<GiftCard>().Remove(giftCard);
}
