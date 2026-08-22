using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Users.ValueObjects;

public sealed class UserAddressId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private UserAddressId(Guid value)
    {
        Value = value;
    }

    public static UserAddressId CreateUnique() => new(Guid.NewGuid());

    public static UserAddressId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
