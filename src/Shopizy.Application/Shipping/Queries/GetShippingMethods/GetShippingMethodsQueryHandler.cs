using ErrorOr;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Shipping.Queries.GetShippingMethods;

public class GetShippingMethodsQueryHandler(IShippingCarrierService shippingCarrierService)
    : IQueryHandler<GetShippingMethodsQuery, ErrorOr<IReadOnlyList<ShippingRateEstimateDto>>>
{
    private readonly IShippingCarrierService _shippingCarrierService = shippingCarrierService;

    public async Task<ErrorOr<IReadOnlyList<ShippingRateEstimateDto>>> Handle(
        GetShippingMethodsQuery request,
        CancellationToken cancellationToken
    )
    {
        var methods = await _shippingCarrierService.GetShippingMethodsAsync(cancellationToken);
        return methods.ToList().AsReadOnly();
    }
}
