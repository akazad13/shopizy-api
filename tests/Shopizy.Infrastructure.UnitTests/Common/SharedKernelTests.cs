using ErrorOr;
using Moq;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Logging;
using Shopizy.SharedKernel.Domain.Models;
using Shouldly;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Common;

public class SharedKernelTests
{
    private class DummyEntity : Entity<UserId>
    {
        public DummyEntity(UserId id)
            : base(id) { }
    }

    private class DummyValueObject : ValueObject
    {
        public string Name { get; }
        public int Age { get; }

        public DummyValueObject(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Age;
        }
    }

    [Fact]
    public void Entity_EqualityAndOperators_ShouldWork()
    {
        var id1 = UserId.CreateUnique();
        var id2 = UserId.CreateUnique();

        var e1 = new DummyEntity(id1);
        var e2 = new DummyEntity(id1);
        var e3 = new DummyEntity(id2);

        (e1 == e2).ShouldBeTrue();
        (e1 != e3).ShouldBeTrue();
        e1.Equals((object)e2).ShouldBeTrue();
        e1.Equals((Entity<UserId>?)null).ShouldBeFalse();
        e1.GetHashCode().ShouldBe(id1.GetHashCode());

        var mockDomainEvent = new Mock<IDomainEvent>();
        e1.AddDomainEvent(mockDomainEvent.Object);
        e1.DomainEvents.Count.ShouldBe(1);

        var popped = e1.PopDomainEvents();
        popped.Count.ShouldBe(1);
        e1.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ValueObject_EqualityAndOperators_ShouldWork()
    {
        var vo1 = new DummyValueObject("Alice", 30);
        var vo2 = new DummyValueObject("Alice", 30);
        var vo3 = new DummyValueObject("Bob", 25);

        (vo1 == vo2).ShouldBeTrue();
        (vo1 != vo3).ShouldBeTrue();
        vo1.Equals((object)vo2).ShouldBeTrue();
        vo1.Equals((object?)null).ShouldBeFalse();
        vo1.Equals((ValueObject?)null).ShouldBeFalse();
        vo1.GetHashCode().ShouldNotBe(0);
    }

    [Fact]
    public void DomainResult_ImplicitConversions_ShouldWork()
    {
        DomainResult<string> successResult = "Hello";
        successResult.IsError.ShouldBeFalse();
        successResult.Value.ShouldBe("Hello");

        ErrorOr<string> errorOrSuccess = successResult;
        errorOrSuccess.IsError.ShouldBeFalse();
        errorOrSuccess.Value.ShouldBe("Hello");

        DomainError error = DomainError.Validation("Error.Code", "Error Description");
        DomainResult<string> errorResult = error;
        errorResult.IsError.ShouldBeTrue();
        errorResult.Error.ShouldBe(error);

        ErrorOr<string> errorOrFailure = errorResult;
        errorOrFailure.IsError.ShouldBeTrue();
    }

    [Fact]
    public void LogSanitizer_Sanitize_ShouldRedactPII()
    {
        LogSanitizer.Sanitize(null).ShouldBe(string.Empty);
        LogSanitizer.Sanitize(string.Empty).ShouldBe(string.Empty);

        var sanitizedEmail = LogSanitizer.Sanitize("Contact user at user@example.com");
        sanitizedEmail.ShouldContain("[email]");

        var sanitizedPhone = LogSanitizer.Sanitize("Call +12345678901 for help");
        sanitizedPhone.ShouldContain("[phone]");

        var sanitizedToken = LogSanitizer.Sanitize("authorization: Bearer mysecrettoken123");
        sanitizedToken.ShouldContain("[token]");
    }
}
