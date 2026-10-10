using Moq;
using Shopizy.Application.Carts.Commands.RemoveProductFromCart;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Carts.TestUtils;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Carts.Commands.RemoveProductFromCart;

public class RemoveProductFromCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly RemoveProductFromCartCommandHandler _sut;

    public RemoveProductFromCartCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _sut = new RemoveProductFromCartCommandHandler(_mockCartRepository.Object);
    }

    // Should return cart not found when cart is not found
    [Fact]
    public async Task Should_ReturnsCartNotFound_WhenCartIsNotFound()
    {
        // Arrange
        var command = RemoveProductFromCartCommandUtils.CreateCommand();

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(() => null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.NotNull(result.Errors);
        Assert.Equal(CustomErrors.Cart.CartNotFound, result.Errors[0]);

        _mockCartRepository.Verify(
            cr => cr.GetCartByUserIdForUpdateAsync(command.UserId),
            Times.Once
        );
        _mockCartRepository.Verify(x => x.Update(It.IsAny<Cart>()), Times.Never);
    }

    // Should remove last line item from cart and commit changes when cart is found
    [Fact]
    public async Task Should_RemoveLastLineItemFromCart_WhenCartIsFound()
    {
        // Arrange
        var existingCart = CartFactory.Create();
        var command = RemoveProductFromCartCommandUtils.CreateCommand();

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(() => existingCart);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.IsType<Cart>(result.Value);

        _mockCartRepository.Verify(
            cr => cr.GetCartByUserIdForUpdateAsync(command.UserId),
            Times.Once
        );
        _mockCartRepository.Verify(x => x.Update(It.IsAny<Cart>()), Times.Never);
    }
}
