namespace Shopizy.Contracts.ProductReview;

/// <summary>
/// Request contract for submitting a product review.
/// </summary>
public record CreateProductReviewRequest(
    decimal Rating,
    string Comment,
    string? Headline = null,
    IReadOnlyList<string>? ImageUrls = null
);
