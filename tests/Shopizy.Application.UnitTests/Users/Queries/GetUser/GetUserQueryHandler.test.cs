using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Users.Queries.GetUser;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Users.Queries.GetUser;

public class GetUserQueryHandlerTestsRefactored
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTestsRefactored()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _handler = new GetUserQueryHandler(_mockUserRepository.Object, _mockOrderRepository.Object);
    }

    private UserDto CreateSampleUserDto(Guid userId) =>
        new(
            UserId.Create(userId),
            "First",
            "Last",
            "test@test.com",
            "Customer",
            null,
            "",
            null,
            0,
            0,
            0,
            0,
            DateTime.UtcNow,
            null
        );
}
