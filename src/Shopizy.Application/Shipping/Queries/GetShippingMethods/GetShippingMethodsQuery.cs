using ErrorOr;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Shipping.Queries.GetShippingMethods;

/// <summary>
/// Query to retrieve available fixed shipping methods.
/// </summary>
public record GetShippingMethodsQuery() : IQuery<ErrorOr<IReadOnlyList<ShippingRateEstimateDto>>>;
