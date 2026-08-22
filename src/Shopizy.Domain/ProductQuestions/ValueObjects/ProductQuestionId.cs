using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductQuestions.ValueObjects;

public sealed class ProductQuestionId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductQuestionId(Guid value)
    {
        Value = value;
    }

    public static ProductQuestionId CreateUnique() => new(Guid.NewGuid());

    public static ProductQuestionId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
