using Mapster;
using Shopizy.Application.Returns.Commands.RequestReturn;
using Shopizy.Contracts.Returns;
using Shopizy.Domain.Returns;
using Shopizy.Domain.Returns.Entities;

namespace Shopizy.Api.Common.Mapping;

public class ReturnMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<
                (Guid UserId, Guid OrderId, RequestReturnRequest request),
                RequestReturnCommand
            >()
            .ConstructUsing(src => new RequestReturnCommand(
                src.OrderId,
                src.UserId,
                src.request.Reason,
                src.request.Items.Select(i => new RequestReturnItemCommand(
                        i.OrderItemId,
                        i.Quantity
                    ))
                    .ToList()
            ));

        config
            .NewConfig<ReturnItem, ReturnItemDto>()
            .Map(dest => dest.ReturnItemId, src => src.Id.Value)
            .Map(dest => dest.OrderItemId, src => src.OrderItemId.Value)
            .Map(dest => dest.Quantity, src => src.Quantity);

        config
            .NewConfig<ReturnRequest, ReturnRequestDto>()
            .Map(dest => dest.ReturnRequestId, src => src.Id.Value)
            .Map(dest => dest.OrderId, src => src.OrderId.Value)
            .Map(dest => dest.UserId, src => src.UserId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items);
    }
}
