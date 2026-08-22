namespace Shopizy.Contracts.ProductReview;

/// <summary>
/// Response contract representing a product review.
/// </summary>
public record ProductReviewResponse(
    Guid ReviewId,
    Guid UserId,
    string UserName,
    decimal Rating,
    string Comment,
    DateTime CreatedOn,
    string? Headline = null,
    bool IsVerifiedPurchase = false,
    int HelpfulVotesCount = 0,
    IReadOnlyList<string>? ImageUrls = null
);
