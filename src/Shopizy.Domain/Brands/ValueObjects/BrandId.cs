using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Brands.ValueObjects;

public sealed class BrandId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private BrandId(Guid value)
    {
        Value = value;
    }

    public static BrandId CreateUnique() => new(Guid.NewGuid());

    public static BrandId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
