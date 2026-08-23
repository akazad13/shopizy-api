using Shopizy.Application.Admin.Queries.GetSalesReport;

namespace Shopizy.Application.Common.Interfaces.Persistence;

/// <summary>
/// Read-only query abstraction for order reporting and dashboard analytics.
/// Follows CQRS read-side separation principles (A2).
/// </summary>
public interface IOrderReader
{
    Task<int> GetTotalOrdersCountAsync();
    Task<int> GetOrdersCountByPeriodAsync(DateTime start, DateTime end);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByPeriodAsync(DateTime start, DateTime end);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsByRevenueAsync(int count);
    Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersBySpendAsync(int count);
}
