using ErrorOr;
using Shopizy.Application.Admin.Queries.GetSalesReport;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Admin.Queries.GetTopProducts;

public record GetTopProductsQuery(int Count = 10)
    : IQuery<ErrorOr<List<TopProductDto>>>,
        ICachableRequest
{
    public string CacheKey => CacheKeys.AdminTopProducts(Count);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
}
