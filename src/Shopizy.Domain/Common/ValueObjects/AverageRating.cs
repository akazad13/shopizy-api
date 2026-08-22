using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Common.ValueObjects;

public sealed class AverageRating : ValueObject
{
    [JsonConstructor]
    private AverageRating(decimal value, int numRatings)
    {
        Value = value;
        NumRatings = numRatings;
    }

    public decimal Value { get; private set; }
    public int NumRatings { get; private set; }

    public static AverageRating CreateNew(decimal rating = 0, int numRatings = 0) =>
        new(rating, numRatings);

    public void AddNewRating(Rating rating) =>
        Value = ((Value * NumRatings) + rating.Value) / ++NumRatings;

    public void RemoveRating(Rating rating)
    {
        if (NumRatings <= 1)
        {
            NumRatings = 0;
            Value = 0;
            return;
        }

        Value = ((Value * NumRatings) - rating.Value) / --NumRatings;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
