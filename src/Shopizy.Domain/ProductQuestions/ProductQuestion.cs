using System.Text.Json.Serialization;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.ProductQuestions.Entities;
using Shopizy.Domain.ProductQuestions.ValueObjects;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductQuestions;

public sealed class ProductQuestion : AggregateRoot<ProductQuestionId, Guid>, IAuditable
{
    public ProductId ProductId { get; private set; } = null!;
    public UserId AskedByUserId { get; private set; } = null!;
    public string Question { get; private set; } = null!;
    public ProductAnswer? Answer { get; private set; }
    public bool IsAnswered { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? ModifiedOn { get; private set; }

    public static ProductQuestion Create(
        ProductId productId,
        UserId askedByUserId,
        string question
    ) => new(ProductQuestionId.CreateUnique(), productId, askedByUserId, question);

    public DomainResult<bool> AddAnswer(UserId answeredByUserId, string answer)
    {
        if (IsAnswered)
        {
            return CustomErrors.ProductQuestion.QuestionAlreadyAnswered;
        }

        Answer = ProductAnswer.Create(answeredByUserId, answer);
        IsAnswered = true;
        ModifiedOn = DateTime.UtcNow;

        return true;
    }

    private ProductQuestion(
        ProductQuestionId id,
        ProductId productId,
        UserId askedByUserId,
        string question
    )
        : base(id)
    {
        ProductId = productId;
        AskedByUserId = askedByUserId;
        Question = question;
        IsAnswered = false;
        CreatedOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private ProductQuestion() { }
}
