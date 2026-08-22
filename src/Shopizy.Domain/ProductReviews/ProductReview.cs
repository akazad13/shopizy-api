using System.Text.Json.Serialization;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.ProductReviews.ValueObjects;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductReviews;

/// <summary>
/// Represents a product review submitted by a user.
/// </summary>
public sealed class ProductReview : AggregateRoot<ProductReviewId, Guid>, IAuditable
{
    /// <summary>
    /// Gets or sets the user identifier who wrote the review.
    /// </summary>
    public UserId UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who wrote the review.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the product identifier being reviewed.
    /// </summary>
    public ProductId ProductId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the rating given to the product.
    /// </summary>
    public Rating Rating { get; set; } = null!;

    /// <summary>
    /// Gets or sets the review headline/title, if provided.
    /// </summary>
    public string? Headline { get; private set; }

    /// <summary>
    /// Gets or sets the review comment.
    /// </summary>
    public string Comment { get; set; } = null!;

    /// <summary>
    /// Gets a value indicating whether this review was submitted by a verified buyer.
    /// </summary>
    public bool IsVerifiedPurchase { get; private set; }

    /// <summary>
    /// Gets the count of helpful upvotes given by other shoppers.
    /// </summary>
    public int HelpfulVotesCount { get; private set; }

    private List<string> _imageUrls = [];

    /// <summary>
    /// Gets the list of customer photos attached to this review.
    /// </summary>
    public IReadOnlyList<string> ImageUrls => (_imageUrls ?? []).AsReadOnly();

    /// <summary>
    /// Gets or sets the date and time when the review was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the review was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Creates a standard product review.
    /// </summary>
    public static ProductReview Create(
        UserId userId,
        ProductId productId,
        Rating rating,
        string comment
    ) => Create(userId, productId, rating, comment, false, null, null);

    /// <summary>
    /// Creates a product review with verified purchase status, headline, and customer photos.
    /// </summary>
    public static ProductReview Create(
        UserId userId,
        ProductId productId,
        Rating rating,
        string comment,
        bool isVerifiedPurchase,
        string? headline = null,
        IEnumerable<string>? imageUrls = null
    )
    {
        var review = new ProductReview(
            ProductReviewId.CreateUnique(),
            userId,
            productId,
            rating,
            comment,
            isVerifiedPurchase,
            headline,
            imageUrls
        );
        review.AddDomainEvent(new Events.ProductReviewCreatedDomainEvent(productId, rating));
        return review;
    }

    /// <summary>
    /// Upvotes this review as helpful.
    /// </summary>
    public void UpvoteHelpful() => HelpfulVotesCount++;

    public void Delete() =>
        AddDomainEvent(new Events.ProductReviewDeletedDomainEvent(ProductId, Rating));

    private ProductReview(
        ProductReviewId productReviewId,
        UserId userId,
        ProductId productId,
        Rating rating,
        string comment,
        bool isVerifiedPurchase,
        string? headline,
        IEnumerable<string>? imageUrls
    )
        : base(productReviewId)
    {
        UserId = userId;
        ProductId = productId;
        Rating = rating;
        Comment = comment;
        IsVerifiedPurchase = isVerifiedPurchase;
        Headline = headline;
        HelpfulVotesCount = 0;
        if (imageUrls is not null)
        {
            _imageUrls.AddRange(imageUrls);
        }
    }

    [JsonConstructor]
    private ProductReview() { }
}
