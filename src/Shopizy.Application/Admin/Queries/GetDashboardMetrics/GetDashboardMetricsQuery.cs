using ErrorOr;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Admin.Queries.GetDashboardMetrics;

public record GetDashboardMetricsQuery() : IQuery<ErrorOr<DashboardMetricsDto>>, ICachableRequest
{
    public string CacheKey => "admin:dashboard-metrics";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
