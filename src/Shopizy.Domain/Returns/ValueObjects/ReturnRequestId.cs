using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Returns.ValueObjects;

public sealed class ReturnRequestId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ReturnRequestId(Guid value)
    {
        Value = value;
    }

    public static ReturnRequestId CreateUnique() => new(Guid.NewGuid());

    public static ReturnRequestId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
