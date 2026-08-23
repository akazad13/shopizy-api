using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Admin.Queries.GetSalesReport;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Orders.Persistence;

/// <summary>
/// Technical implementation of read-only analytics and reporting queries for Orders.
/// </summary>
public class OrderReader(AppDbContext dbContext) : IOrderReader
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<int> GetTotalOrdersCountAsync() => _dbContext.Orders.CountAsync();

    public Task<int> GetOrdersCountByPeriodAsync(DateTime start, DateTime end) =>
        _dbContext.Orders.Where(o => o.CreatedOn >= start && o.CreatedOn <= end).CountAsync();

    public async Task<decimal> GetTotalRevenueAsync()
    {
        var items = await _dbContext
            .Orders.AsNoTracking()
            .SelectMany(o => o.OrderItems)
            .Select(i => new { i.UnitPrice.Amount, i.Quantity })
            .ToListAsync();

        return items.Sum(i => i.Amount * i.Quantity);
    }

    public async Task<decimal> GetRevenueByPeriodAsync(DateTime start, DateTime end)
    {
        var items = await _dbContext
            .Orders.AsNoTracking()
            .Where(o => o.CreatedOn >= start && o.CreatedOn <= end)
            .SelectMany(o => o.OrderItems)
            .Select(i => new { i.UnitPrice.Amount, i.Quantity })
            .ToListAsync();

        return items.Sum(i => i.Amount * i.Quantity);
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsByRevenueAsync(int count)
    {
        var items = await _dbContext
            .Orders.AsNoTracking()
            .SelectMany(o => o.OrderItems)
            .Select(i => new
            {
                i.Name,
                i.Quantity,
                Amount = i.UnitPrice.Amount,
            })
            .ToListAsync();

        return items
            .GroupBy(i => i.Name)
            .Select(g => new TopProductDto(
                g.Key,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.Amount * i.Quantity)
            ))
            .OrderByDescending(p => p.Revenue)
            .Take(count)
            .ToList();
    }

    public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersBySpendAsync(int count)
    {
        var customerSpend = await _dbContext
            .Orders.AsSingleQuery()
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalSpend = g.Sum(o =>
                    o.OrderItems.Sum(i => i.UnitPrice.Amount * i.Quantity) + o.DeliveryCharge.Amount
                ),
            })
            .OrderByDescending(x => x.TotalSpend)
            .Take(count)
            .ToListAsync();

        var userIds = customerSpend.Select(x => x.UserId).ToList();
        var users = await _dbContext
            .Users.Where(u => userIds.Contains(u.Id))
            .AsNoTracking()
            .ToListAsync();

        return customerSpend
            .Join(
                users,
                cs => cs.UserId,
                u => u.Id,
                (cs, u) => new TopCustomerDto(u.Id.Value, u.FirstName, u.LastName, cs.TotalSpend)
            )
            .ToList();
    }
}
