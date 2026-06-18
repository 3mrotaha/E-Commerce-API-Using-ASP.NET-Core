using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class CartService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<ICartItemRepository> _cartItems = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IDistributedCache> _cache = new();
    private readonly CartService _sut;

    public CartService_Test()
    {
        _uow.SetupGet(u => u.Carts).Returns(_carts.Object);
        _uow.SetupGet(u => u.CartItems).Returns(_cartItems.Object);
        _uow.SetupGet(u => u.Products).Returns(_products.Object);
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sut = new CartService(_uow.Object, _mapper.Object, NullLogger<CartService>.Instance, _cache.Object);
    }

    // Forces a cache hit by returning the serialized cart the service would read back.
    private void CacheReturnsCart(CartResponseDto cart) =>
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cart)));

    private static CartResponseDto CartWith(Guid userId, params CartItemResponseDto[] items) =>
        new(Guid.NewGuid(), userId, items, DateTime.UtcNow, DateTime.UtcNow);

    private static CartItemResponseDto Item(int id, Guid productId, int qty, decimal price = 5m) =>
        new(id, Guid.NewGuid(), productId, "P", qty, price, qty * price, DateTime.UtcNow);

    [Fact]
    public async Task GetCartByUserAsync_WhenCached_ShouldReturnCachedCart()
    {
        var userId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId));

        var result = await _sut.GetCartByUserAsync(userId);

        result.IsSuccess.Should().BeTrue();
        _carts.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task GetCartByUserAsync_WhenNotCachedButExistsInDb_ShouldBuildAndReturnCart()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(new[] { cart });
        _cartItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CartItem, bool>>>())).ReturnsAsync(Array.Empty<CartItem>());
        _mapper.Setup(m => m.Map<CartResponseDto>(It.IsAny<Cart>())).Returns(CartWith(userId));

        var result = await _sut.GetCartByUserAsync(userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetCartByUserAsync_WhenNoCart_ShouldReturnNotFound()
    {
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(Array.Empty<Cart>());

        var result = await _sut.GetCartByUserAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AddItemAsync_WhenQuantityNotPositive_ShouldReturnBadRequest()
    {
        var result = await _sut.AddItemAsync(Guid.NewGuid(), new AddCartItemDto(Guid.NewGuid(), 0));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddItemAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.AddItemAsync(Guid.NewGuid(), new AddCartItemDto(Guid.NewGuid(), 1));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AddItemAsync_WhenCartCannotBeCreated_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product { QuantityInStock = 10 });
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(Array.Empty<Cart>());
        _carts.Setup(r => r.AddAsync(It.IsAny<Cart>())).ReturnsAsync((Cart?)null); // creation fails

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto(Guid.NewGuid(), 1));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddItemAsync_WhenInsufficientStock_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId)); // empty cart from cache
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 1 });

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto(productId, 5));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddItemAsync_WhenNewItem_ShouldAddAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 10, UnitPrice = 5m });
        _cartItems.Setup(r => r.AddAsync(It.IsAny<CartItem>())).ReturnsAsync((CartItem ci) => ci);

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto(productId, 2));

        result.IsSuccess.Should().BeTrue();
        _cartItems.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WhenItemAlreadyInCart_ShouldMergeQuantityAndUpdate()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(7, productId, qty: 1)));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 10, UnitPrice = 5m });
        _cartItems.Setup(r => r.UpdateAsync(It.IsAny<CartItem>())).ReturnsAsync((CartItem ci) => ci);

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto(productId, 2));

        result.IsSuccess.Should().BeTrue();
        _cartItems.Verify(r => r.UpdateAsync(It.Is<CartItem>(ci => ci.Quantity == 3)), Times.Once); // 1 existing + 2 added
    }

    [Fact]
    public async Task AddItemAsync_WhenAddRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 10, UnitPrice = 5m });
        _cartItems.Setup(r => r.AddAsync(It.IsAny<CartItem>())).ReturnsAsync((CartItem?)null);

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto(productId, 2));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenQuantityNotPositive_ShouldReturnBadRequest()
    {
        var result = await _sut.UpdateItemAsync(Guid.NewGuid(), 1, new UpdateCartItemDto(0));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenCartNotFound_ShouldReturnNotFound()
    {
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(Array.Empty<Cart>());

        var result = await _sut.UpdateItemAsync(Guid.NewGuid(), 1, new UpdateCartItemDto(2));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenItemNotInCart_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId)); // empty cart

        var result = await _sut.UpdateItemAsync(userId, 99, new UpdateCartItemDto(2));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(5, productId, qty: 1)));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateItemAsync(userId, 5, new UpdateCartItemDto(2));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenInsufficientStock_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(5, productId, qty: 1)));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 1 });

        var result = await _sut.UpdateItemAsync(userId, 5, new UpdateCartItemDto(10));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(5, productId, qty: 1)));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 10, UnitPrice = 5m });
        _cartItems.Setup(r => r.UpdateAsync(It.IsAny<CartItem>())).ReturnsAsync((CartItem?)null);

        var result = await _sut.UpdateItemAsync(userId, 5, new UpdateCartItemDto(3));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateItemAsync_WhenValid_ShouldUpdateAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(5, productId, qty: 1)));
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId, Name = "P", QuantityInStock = 10, UnitPrice = 5m });
        _cartItems.Setup(r => r.UpdateAsync(It.IsAny<CartItem>())).ReturnsAsync((CartItem ci) => ci);

        var result = await _sut.UpdateItemAsync(userId, 5, new UpdateCartItemDto(3));

        result.IsSuccess.Should().BeTrue();
        _cartItems.Verify(r => r.UpdateAsync(It.Is<CartItem>(ci => ci.Quantity == 3)), Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenCartNotFound_ShouldReturnNotFound()
    {
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(Array.Empty<Cart>());

        var result = await _sut.RemoveItemAsync(Guid.NewGuid(), 1);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenItemNotInCart_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId));

        var result = await _sut.RemoveItemAsync(userId, 42);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenItemPresent_ShouldDeleteAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(5, Guid.NewGuid(), qty: 1)));

        var result = await _sut.RemoveItemAsync(userId, 5);

        result.IsSuccess.Should().BeTrue();
        _cartItems.Verify(r => r.DeleteAsync(It.Is<CartItem>(ci => ci.Id == 5)), Times.Once);
    }

    [Fact]
    public async Task EmptyCartAsync_WhenCartNotFound_ShouldReturnNotFound()
    {
        _carts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Cart, bool>>>())).ReturnsAsync(Array.Empty<Cart>());

        var result = await _sut.EmptyCartAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task EmptyCartAsync_WhenCartHasItems_ShouldDeleteAllAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        CacheReturnsCart(CartWith(userId, Item(1, Guid.NewGuid(), 1), Item(2, Guid.NewGuid(), 2)));

        var result = await _sut.EmptyCartAsync(userId);

        result.IsSuccess.Should().BeTrue();
        _cartItems.Verify(r => r.DeleteAsync(It.IsAny<CartItem>()), Times.Exactly(2));
    }
}
