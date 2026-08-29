using ErrorOr;
using FluentValidation.TestHelper;
using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Users.Queries.GetUser;
using Shopizy.Application.Users.Queries.GetUsers;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.Orders;
using Shopizy.Domain.Orders.Entities;
using Shopizy.Domain.Orders.Enums;
using Shopizy.Domain.Orders.ValueObjects;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shouldly;

namespace Shopizy.Application.UnitTests.Users.Queries.GetUser;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _handler = new GetUserQueryHandler(_mockUserRepository.Object, _mockOrderRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUserNotFound()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>()))
            .ReturnsAsync((User?)null);

        var query = new GetUserQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CustomErrors.User.UserNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnUserDtoWithOrderStatistics()
    {
        // Arrange
        var user = User.Create(
            "John",
            "Doe",
            "john@example.com",
            "hash",
            UserRole.Customer,
            new List<PermissionId>()
        );
        _mockUserRepository
            .Setup(r => r.GetUserByIdAsync(It.Is<UserId>(id => id.Value == user.Id.Value)))
            .ReturnsAsync(user);

        var address = Address.CreateNew("123 Main St", "City", "State", "12345", "Country");
        var deliveryCharge = Price.CreateNew(10m, Currency.usd);
        var order1 = Order.Create(user.Id, "", 0, deliveryCharge, address, new List<OrderItem>());
        var order2 = Order.Create(user.Id, "", 0, deliveryCharge, address, new List<OrderItem>());
        order2.CancelOrder("Customer request");
        order2.UpdateOrderStatus(OrderStatus.Refunded);

        _mockOrderRepository
            .Setup(r => r.GetOrdersByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order1, order2 });

        var query = new GetUserQuery(user.Id.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(user.Id);
        result.Value.Email.ShouldBe("john@example.com");
        result.Value.Role.ShouldBe("Customer");
        result.Value.TotalOrders.ShouldBe(2);
        result.Value.TotalReturns.ShouldBe(1);
    }

    [Fact]
    public void GetUsersQueryValidator_ShouldValidatePagination()
    {
        var validator = new GetUsersQueryValidator();

        var validQuery = new GetUsersQuery(1, 20);
        validator.TestValidate(validQuery).ShouldNotHaveAnyValidationErrors();

        var invalidQuery = new GetUsersQuery(0, 101);
        var result = validator.TestValidate(invalidQuery);
        result.ShouldHaveValidationErrorFor(q => q.PageNumber);
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
