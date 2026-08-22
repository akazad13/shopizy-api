using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductReviews.ValueObjects;

public sealed class ProductReviewId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductReviewId(Guid value)
    {
        Value = value;
    }

    public static ProductReviewId CreateUnique() => new(Guid.NewGuid());

    public static ProductReviewId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
