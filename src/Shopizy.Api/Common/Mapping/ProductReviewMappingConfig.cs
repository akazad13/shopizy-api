using Mapster;
using Shopizy.Application.ProductReviews.Commands.CreateProductReview;
using Shopizy.Contracts.ProductReview;
using Shopizy.Domain.ProductReviews;

namespace Shopizy.Api.Common.Mapping;

public class ProductReviewMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<
                (Guid UserId, Guid ProductId, CreateProductReviewRequest request),
                CreateProductReviewCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Rating, src => src.request.Rating)
            .Map(dest => dest.Comment, src => src.request.Comment)
            .Map(dest => dest.Headline, src => src.request.Headline)
            .Map(dest => dest.ImageUrls, src => src.request.ImageUrls);

#pragma warning disable CS8625
        config
            .NewConfig<ProductReview, ProductReviewResponse>()
            .Map(dest => dest.ReviewId, src => src.Id.Value)
            .Map(dest => dest.UserId, src => src.UserId.Value)
            .Map(
                dest => dest.UserName,
                src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty
            )
            .Map(dest => dest.Rating, src => src.Rating.Value)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.CreatedOn, src => src.CreatedOn)
            .Map(dest => dest.Headline, src => src.Headline)
            .Map(dest => dest.IsVerifiedPurchase, src => src.IsVerifiedPurchase)
            .Map(dest => dest.HelpfulVotesCount, src => src.HelpfulVotesCount)
            .Map(dest => dest.ImageUrls, src => src.ImageUrls);
#pragma warning restore CS8625
    }
}
