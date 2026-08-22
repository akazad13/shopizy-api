using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Returns.ValueObjects;

public sealed class ReturnItemId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ReturnItemId(Guid value)
    {
        Value = value;
    }

    public static ReturnItemId CreateUnique() => new(Guid.NewGuid());

    public static ReturnItemId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
