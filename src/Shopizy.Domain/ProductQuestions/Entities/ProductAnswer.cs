using System.Text.Json.Serialization;
using Shopizy.Domain.ProductQuestions.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductQuestions.Entities;

public sealed class ProductAnswer : Entity<ProductAnswerId>
{
    public UserId AnsweredByUserId { get; private set; } = null!;
    public string Answer { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    public static ProductAnswer Create(UserId answeredByUserId, string answer) =>
        new(ProductAnswerId.CreateUnique(), answeredByUserId, answer);

    private ProductAnswer(ProductAnswerId id, UserId answeredByUserId, string answer)
        : base(id)
    {
        AnsweredByUserId = answeredByUserId;
        Answer = answer;
        CreatedOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private ProductAnswer() { }
}
