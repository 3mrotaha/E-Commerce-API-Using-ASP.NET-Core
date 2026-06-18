using System.Linq.Expressions;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class OrderService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICardPaymentMethodRepository> _cards = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IOrderItemRepository> _orderItems = new();
    private readonly Mock<IPaymentRecordRepository> _paymentRecords = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly OrderService _sut;

    public OrderService_Test()
    {
        _uow.SetupGet(u => u.CardPaymentMethods).Returns(_cards.Object);
        _uow.SetupGet(u => u.Products).Returns(_products.Object);
        _uow.SetupGet(u => u.Orders).Returns(_orders.Object);
        _uow.SetupGet(u => u.OrderItems).Returns(_orderItems.Object);
        _uow.SetupGet(u => u.PaymentRecords).Returns(_paymentRecords.Object);
        // Order responses are mapped from whatever Order the service finally produces.
        _mapper.Setup(m => m.Map<OrderResponseDto>(It.IsAny<Order>()))
               .Returns((Order o) => new OrderResponseDto(o.Id, o.UserId, o.PaymentId, o.OrderState,
                   o.HasDiscount, o.DiscountValue, Array.Empty<OrderItemResponseDto>(), o.CreatedAt));
        _sut = new OrderService(_uow.Object, _mapper.Object, NullLogger<OrderService>.Instance);
    }

    private static CreateOrderDto OrderDto(Guid productId, int qty = 2, bool hasDiscount = false, decimal? discount = null, Guid? paymentMethodId = null) =>
        new(paymentMethodId ?? Guid.NewGuid(), new[] { new CreateOrderItemDto(productId, qty) }, hasDiscount, discount);

    // Wires the full happy path; individual tests override single repos to exercise failure branches.
    private (Guid userId, Guid productId, Product product) ArrangeHappyPath(int stock = 10, decimal unitPrice = 5m)
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var pmId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Widget", QuantityInStock = stock, UnitPrice = unitPrice };

        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CardPaymentMethod { Id = pmId, UserId = userId });
        _products.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _orders.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        _orders.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        _orderItems.Setup(r => r.AddAsync(It.IsAny<OrderItem>())).ReturnsAsync((OrderItem oi) => oi);
        _products.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
        _paymentRecords.Setup(r => r.AddAsync(It.IsAny<PaymentRecord>())).ReturnsAsync((PaymentRecord pr) => pr);
        return (userId, productId, product);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenNoItems_ShouldReturnBadRequest()
    {
        var dto = new CreateOrderDto(Guid.NewGuid(), Array.Empty<CreateOrderItemDto>(), false, null);

        var result = await _sut.CreateOrderAsync(Guid.NewGuid(), dto);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenDiscountFlaggedButValueMissing_ShouldReturnBadRequest()
    {
        var dto = OrderDto(Guid.NewGuid(), hasDiscount: true, discount: null);

        var result = await _sut.CreateOrderAsync(Guid.NewGuid(), dto);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenPaymentMethodNotFound_ShouldReturnNotFound()
    {
        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.CreateOrderAsync(Guid.NewGuid(), OrderDto(Guid.NewGuid()));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenPaymentMethodOwnedByAnotherUser_ShouldThrowUnauthorized()
    {
        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
              .ReturnsAsync(new CardPaymentMethod { UserId = Guid.NewGuid() }); // different owner

        var act = () => _sut.CreateOrderAsync(Guid.NewGuid(), OrderDto(Guid.NewGuid()));

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        var (userId, _, _) = ArrangeHappyPath();
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(Guid.NewGuid()));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenItemQuantityNotPositive_ShouldReturnBadRequest()
    {
        var (userId, productId, _) = ArrangeHappyPath();

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId, qty: 0));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenInsufficientStock_ShouldReturnBadRequest()
    {
        var (userId, productId, _) = ArrangeHappyPath(stock: 1);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId, qty: 5));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenOrderRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var (userId, productId, _) = ArrangeHappyPath();
        _orders.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order?)null);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenOrderItemRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var (userId, productId, _) = ArrangeHappyPath();
        _orderItems.Setup(r => r.AddAsync(It.IsAny<OrderItem>())).ReturnsAsync((OrderItem?)null);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenPaymentRecordRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var (userId, productId, _) = ArrangeHappyPath();
        _paymentRecords.Setup(r => r.AddAsync(It.IsAny<PaymentRecord>())).ReturnsAsync((PaymentRecord?)null);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldDecrementStockLinkPaymentAndSucceed()
    {
        var (userId, productId, product) = ArrangeHappyPath(stock: 10, unitPrice: 5m);
        Order? capturedOrder = null;
        PaymentRecord? capturedPayment = null;
        _orders.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o).Callback<Order>(o => capturedOrder = o);
        _paymentRecords.Setup(r => r.AddAsync(It.IsAny<PaymentRecord>()))
                       .ReturnsAsync((PaymentRecord pr) => pr).Callback<PaymentRecord>(pr => capturedPayment = pr);

        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId, qty: 3));

        result.IsSuccess.Should().BeTrue();
        product.QuantityInStock.Should().Be(7);              // 10 - 3
        capturedPayment!.Amount.Should().Be(15m);            // 3 * 5, no discount
        capturedOrder!.PaymentId.Should().Be(capturedPayment.Id); // order linked to its payment
        _products.Verify(r => r.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenDiscountExceedsSubtotal_ShouldCapDiscountAtSubtotal()
    {
        var (userId, productId, _) = ArrangeHappyPath(stock: 10, unitPrice: 5m);
        PaymentRecord? capturedPayment = null;
        _paymentRecords.Setup(r => r.AddAsync(It.IsAny<PaymentRecord>()))
                       .ReturnsAsync((PaymentRecord pr) => pr).Callback<PaymentRecord>(pr => capturedPayment = pr);

        // subtotal = 3 * 5 = 15, requested discount 1000 → capped to 15 → final amount 0
        var result = await _sut.CreateOrderAsync(userId, OrderDto(productId, qty: 3, hasDiscount: true, discount: 1000m));

        result.IsSuccess.Should().BeTrue();
        capturedPayment!.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await _sut.DeleteOrderAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenFound_ShouldDeleteAndReturnSuccess()
    {
        var order = new Order { Id = Guid.NewGuid() };
        _orders.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var result = await _sut.DeleteOrderAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
        _orders.Verify(r => r.DeleteAsync(order), Times.Once);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await _sut.GetOrderByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var order = new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _orders.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var act = () => _sut.GetOrderByIdAsync(order.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOwner_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var order = new Order { Id = Guid.NewGuid(), UserId = userId };
        _orders.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>())).ReturnsAsync(Array.Empty<OrderItem>());

        var result = await _sut.GetOrderByIdAsync(order.Id, userId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetOrdersByUserAsync_WhenCalled_ShouldReturnOrdersNewestFirst()
    {
        var userId = Guid.NewGuid();
        var older = new Order { Id = Guid.NewGuid(), UserId = userId, CreatedAt = new DateTime(2026, 1, 1) };
        var newer = new Order { Id = Guid.NewGuid(), UserId = userId, CreatedAt = new DateTime(2026, 6, 1) };
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(new[] { older, newer });
        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>())).ReturnsAsync(Array.Empty<OrderItem>());

        var result = await _sut.GetOrdersByUserAsync(userId);

        result.Value!.Select(o => o.Id).Should().ContainInOrder(newer.Id, older.Id);
    }

    [Fact]
    public async Task UpdateOrderStateAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _orders.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await _sut.UpdateOrderStateAsync(Guid.NewGuid(), new UpdateOrderStateDto(OrderState.SHIPPED));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateOrderStateAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var order = new Order { Id = Guid.NewGuid() };
        _orders.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orders.Setup(r => r.UpdateAsync(order)).ReturnsAsync((Order?)null);

        var result = await _sut.UpdateOrderStateAsync(order.Id, new UpdateOrderStateDto(OrderState.SHIPPED));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateOrderStateAsync_WhenValid_ShouldSetStateAndReturnSuccess()
    {
        var order = new Order { Id = Guid.NewGuid(), OrderState = OrderState.PENDING };
        _orders.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orders.Setup(r => r.UpdateAsync(order)).ReturnsAsync(order);
        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>())).ReturnsAsync(Array.Empty<OrderItem>());

        var result = await _sut.UpdateOrderStateAsync(order.Id, new UpdateOrderStateDto(OrderState.SHIPPED));

        result.IsSuccess.Should().BeTrue();
        order.OrderState.Should().Be(OrderState.SHIPPED);
    }
}
