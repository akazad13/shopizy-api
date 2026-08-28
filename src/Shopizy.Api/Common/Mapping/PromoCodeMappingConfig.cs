using Mapster;
using Shopizy.Application.PromoCodes.Commands.CreatePromoCode;
using Shopizy.Application.PromoCodes.Commands.UpdatePromoCode;
using Shopizy.Contracts.PromoCode;
using Shopizy.Domain.PromoCodes;

namespace Shopizy.Api.Common.Mapping;

public class PromoCodeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<CreatePromoCodeRequest, CreatePromoCodeCommand>()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Discount, src => src.Discount)
            .Map(dest => dest.IsPercentage, src => src.IsPercentage)
            .Map(dest => dest.IsActive, src => src.IsActive);

        config
            .NewConfig<(Guid PromoCodeId, UpdatePromoCodeRequest request), UpdatePromoCodeCommand>()
            .Map(dest => dest.PromoCodeId, src => src.PromoCodeId)
            .Map(dest => dest.Code, src => src.request.Code)
            .Map(dest => dest.Description, src => src.request.Description)
            .Map(dest => dest.Discount, src => src.request.Discount)
            .Map(dest => dest.IsPercentage, src => src.request.IsPercentage)
            .Map(dest => dest.IsActive, src => src.request.IsActive);

        config
            .NewConfig<PromoCode, PromoCodeResponse>()
            .ConstructUsing(src => new PromoCodeResponse(
                src.Id.Value,
                src.Code,
                src.Description,
                src.Discount,
                src.IsPercentage,
                src.IsActive,
                src.NumOfTimeUsed,
                src.CreatedOn
            ));
    }
}
