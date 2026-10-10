using Moq;
using Shopizy.Application.Carts.Commands.AddProductToCart;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Carts.TestUtils;
using Shopizy.Application.UnitTests.TestUtils.Constants;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.Entities;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;

namespace Shopizy.Application.UnitTests.Carts.Commands.AddProductToCart;

public class AddProductToCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly AddProductToCartCommandHandler _sut;

    public AddProductToCartCommandHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockProductRepository = new Mock<IProductRepository>();

        _sut = new AddProductToCartCommandHandler(
            _mockCartRepository.Object,
            _mockProductRepository.Object
        );
    }

    // Should return error when cart is not found
    [Fact]
    public async Task Should_ReturnError_WhenCartIdIsInvalid()
    {
        // Arrange
        var command = AddProductToCartCommandUtils.CreateCommand();

        _mockCartRepository
            .Setup(x => x.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(() => null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Equal(CustomErrors.Cart.CartNotFound, result.Errors[0]);
    }

    // Should add new line item to cart when product is not already present
    [Fact]
    public async Task Should_AddNewLineItemToCart_WhenProductIsNotAlreadyPresent()
    {
        // Arrange
        var existingCart = CartFactory.Create();
        var command = AddProductToCartCommandUtils.CreateCommand();

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(existingCart);
        _mockProductRepository
            .Setup(x => x.IsProductExistAsync(It.IsAny<ProductId>()))
            .ReturnsAsync(true);

        // Act
        var cart = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(cart.IsError);
        Assert.NotNull(cart.Value);
        Assert.IsType<Cart>(cart.Value);
        Assert.Equal(existingCart, cart.Value);
        Assert.Single(cart.Value.CartItems);
        Assert.IsType<ProductId>(cart.Value.CartItems[0].ProductId);
        Assert.Contains(cart.Value.CartItems, li => li.ProductId == command.ProductId);
        Assert.Equal(command.Quantity, cart.Value.CartItems[0].Quantity);
        _mockCartRepository.Verify(x => x.Update(It.IsAny<Cart>()), Times.Never);
    }

    // Should add new line item for another product
    [Fact]
    public async Task Should_AddNewLineItem_ForAnotherProduct()
    {
        // Arrange
        var existingCart = CartFactory.Create();
        existingCart.AddLineItem(
            CartItem.Create(
                ProductId.CreateUnique(),
                Constants.CartItem.Color,
                Constants.CartItem.Size,
                1
            )
        );

        var command = AddProductToCartCommandUtils.CreateCommand();

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(existingCart);

        _mockProductRepository
            .Setup(x => x.IsProductExistAsync(It.IsAny<ProductId>()))
            .ReturnsAsync(true);

        // Act
        var cart = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(cart.IsError);
        Assert.NotNull(cart.Value);
        Assert.IsType<Cart>(cart.Value);
        Assert.Equal(existingCart, cart.Value);
        Assert.Equal(2, cart.Value.CartItems.Count);
        Assert.IsType<ProductId>(cart.Value.CartItems[0].ProductId);
        Assert.Equal(1, cart.Value.CartItems[0].Quantity);
        _mockCartRepository.Verify(x => x.Update(It.IsAny<Cart>()), Times.Never);
        Assert.Contains(
            cart.Value.CartItems,
            li => li.ProductId == command.ProductId && li.Quantity == command.Quantity
        );
    }

    [Fact]
    public async Task Should_ReturnError_WhenProductAlreadyExistsInCartWithSameColorAndSize()
    {
        var command = AddProductToCartCommandUtils.CreateCommand();
        var existingCart = CartFactory.Create();
        existingCart.AddLineItem(
            CartItem.Create(command.ProductId, command.Color, command.Size, 1)
        );

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(existingCart);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(CustomErrors.Cart.ProductAlreadyExistInCart, result.FirstError);
    }

    [Fact]
    public async Task Should_ReturnError_WhenProductDoesNotExist()
    {
        var command = AddProductToCartCommandUtils.CreateCommand();
        var existingCart = CartFactory.Create();

        _mockCartRepository
            .Setup(cr => cr.GetCartByUserIdForUpdateAsync(command.UserId))
            .ReturnsAsync(existingCart);
        _mockProductRepository
            .Setup(x => x.IsProductExistAsync(It.IsAny<ProductId>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(CustomErrors.Product.ProductNotFound, result.FirstError);
    }
}
