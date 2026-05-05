using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderDto orderCreateDto)
    {
        _logger.LogDebug("Creating order for user {UserId} with {ItemCount} items", userId, orderCreateDto.Items?.Count() ?? 0);

        var orderValidation = ValidateOrderCreateDto(orderCreateDto);
        if (orderValidation != null)
        {
            _logger.LogError("Cannot create order for user {UserId}; order data is invalid", userId);
            return Result<OrderResponseDto>.BadRequest(orderValidation);
        }

        var paymentMethod = await _unitOfWork.CardPaymentMethods.GetByIdAsync(orderCreateDto.PaymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogError("Cannot create order for user {UserId}; payment method {PaymentMethodId} was not found", userId, orderCreateDto.PaymentMethodId);
            return Result<OrderResponseDto>.NotFound($"Payment method with id '{orderCreateDto.PaymentMethodId}' was not found.");
        }

        if (paymentMethod.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to create an order with payment method {PaymentMethodId} owned by user {OwnerUserId}", userId, orderCreateDto.PaymentMethodId, paymentMethod.UserId);
            throw new UnautherizedException("You are not authorized to create an order with this payment method.");
        }

        var productIds = orderCreateDto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = new Dictionary<Guid, Product>();

        foreach (var productId in productIds)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogError("Cannot create order for user {UserId}; product {ProductId} was not found", userId, productId);
                return Result<OrderResponseDto>.NotFound($"Product with id '{productId}' was not found.");
            }

            products[productId] = product;
        }

        decimal subTotal = 0m;
        foreach (var item in orderCreateDto.Items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogError("Cannot create order for user {UserId}; product {ProductId} has invalid quantity {Quantity}", userId, item.ProductId, item.Quantity);
                return Result<OrderResponseDto>.BadRequest("Order item quantity must be greater than zero.");
            }

            var product = products[item.ProductId];
            if (product.QuantityInStock < item.Quantity)
            {
                _logger.LogError("Cannot create order for user {UserId}; insufficient stock for product {ProductId}", userId, item.ProductId);
                return Result<OrderResponseDto>.BadRequest($"Insufficient stock for product '{product.Name}'.");
            }

            subTotal += item.Quantity * product.UnitPrice;
        }

        var discountValue = CalculateDiscount(orderCreateDto.HasDiscount, orderCreateDto.DiscountValue, subTotal);
        var finalAmount = subTotal - discountValue;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PaymentId = Guid.Empty,
            OrderState = OrderState.PLACED,
            HasDiscount = orderCreateDto.HasDiscount,
            DiscountValue = orderCreateDto.HasDiscount ? discountValue : null,
            CreatedAt = DateTime.UtcNow
        };

        var createdOrder = await _unitOfWork.Orders.AddAsync(order);
        if (createdOrder == null)
        {
            _logger.LogError("Failed to create order for user {UserId}", userId);
            return Result<OrderResponseDto>.BadRequest("Failed to create order.");
        }

        var createdOrderItems = new List<OrderItem>();
        foreach (var item in orderCreateDto.Items)
        {
            var product = products[item.ProductId];

            var orderItem = new OrderItem
            {
                UserId = userId,
                OrderId = createdOrder.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.UnitPrice
            };

            var createdOrderItem = await _unitOfWork.OrderItems.AddAsync(orderItem);
            if (createdOrderItem == null)
            {
                _logger.LogError("Failed to create order item for order {OrderId} and product {ProductId}", createdOrder.Id, item.ProductId);
                return Result<OrderResponseDto>.BadRequest("Failed to create order item.");
            }

            product.QuantityInStock -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product);

            createdOrderItem.Product = product;
            createdOrderItems.Add(createdOrderItem);
        }

        var paymentRecord = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = createdOrder.Id,
            PaymentMethodId = paymentMethod.Id,
            Amount = finalAmount,
            CreatedAt = DateTime.UtcNow,
            PaymentMethod = paymentMethod
        };

        var createdPaymentRecord = await _unitOfWork.PaymentRecords.AddAsync(paymentRecord);
        if (createdPaymentRecord == null)
        {
            _logger.LogError("Failed to create payment record for order {OrderId}", createdOrder.Id);
            return Result<OrderResponseDto>.BadRequest("Failed to create payment record for order.");
        }

        createdOrder.PaymentId = createdPaymentRecord.Id;
        var updatedOrder = await _unitOfWork.Orders.UpdateAsync(createdOrder);
        if (updatedOrder == null)
        {
            _logger.LogError("Failed to update order payment id for order {OrderId}", createdOrder.Id);
            return Result<OrderResponseDto>.BadRequest("Failed to finalize order.");
        }

        updatedOrder.OrderItems = createdOrderItems;
        _logger.LogInformation("Created order {OrderId} for user {UserId} with payment {PaymentId}", updatedOrder.Id, userId, createdPaymentRecord.Id);
        return Result<OrderResponseDto>.Success(_mapper.Map<OrderResponseDto>(updatedOrder));
    }

    public async Task<Result<bool>> DeleteOrderAsync(Guid orderId)
    {
        _logger.LogDebug("Deleting order {OrderId}", orderId);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogError("Cannot delete order {OrderId}; order was not found", orderId);
            return Result<bool>.NotFound($"Order with id '{orderId}' was not found.");
        }

        await _unitOfWork.Orders.DeleteAsync(order);
        _logger.LogInformation("Deleted order {OrderId}", orderId);
        return Result<bool>.Success(true);
    }

    public async Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        _logger.LogDebug("Getting order {OrderId} for user {UserId}", orderId, userId);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogError("Order {OrderId} was not found for user {UserId}", orderId, userId);
            return Result<OrderResponseDto>.NotFound($"Order with id '{orderId}' was not found.");
        }

        if (order.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to access order {OrderId} owned by user {OwnerUserId}", userId, orderId, order.UserId);
            throw new UnautherizedException("You are not authorized to access this order.");
        }

        _logger.LogInformation("Retrieved order {OrderId} for user {UserId}", orderId, userId);
        return Result<OrderResponseDto>.Success(await BuildOrderResponseAsync(order));
    }

    public async Task<Result<IEnumerable<OrderResponseDto>>> GetOrdersByUserAsync(Guid userId)
    {
        _logger.LogDebug("Getting orders for user {UserId}", userId);

        var orders = (await _unitOfWork.Orders.FindAsync(o => o.UserId == userId)).OrderByDescending(o => o.CreatedAt).ToList();

        var responses = new List<OrderResponseDto>(orders.Count);
        foreach (var order in orders)
        {
            responses.Add(await BuildOrderResponseAsync(order));
        }

        _logger.LogInformation("Retrieved {OrderCount} orders for user {UserId}", responses.Count, userId);
        return Result<IEnumerable<OrderResponseDto>>.Success(responses);
    }

    public async Task<Result<OrderResponseDto>> UpdateOrderStateAsync(Guid orderId, UpdateOrderStateDto orderStateDto)
    {
        _logger.LogDebug("Updating order {OrderId} state to {OrderState}", orderId, orderStateDto.OrderState);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogError("Cannot update order {OrderId}; order was not found", orderId);
            return Result<OrderResponseDto>.NotFound($"Order with id '{orderId}' was not found.");
        }

        order.OrderState = orderStateDto.OrderState;

        var updatedOrder = await _unitOfWork.Orders.UpdateAsync(order);
        if (updatedOrder == null)
        {
            _logger.LogError("Failed to update order state for order {OrderId}", orderId);
            return Result<OrderResponseDto>.BadRequest("Failed to update order state.");
        }

        _logger.LogInformation("Updated order {OrderId} state to {OrderState}", orderId, orderStateDto.OrderState);
        return Result<OrderResponseDto>.Success(await BuildOrderResponseAsync(updatedOrder));
    }

    private async Task<OrderResponseDto> BuildOrderResponseAsync(Order order)
    {
        var orderItems = (await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == order.Id)).ToList();

        var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();
        if (productIds.Count > 0)
        {
            var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
            var productLookup = products.ToDictionary(p => p.Id, p => p);

            foreach (var item in orderItems)
            {
                if (productLookup.TryGetValue(item.ProductId, out var product))
                {
                    item.Product = product;
                }
            }
        }

        order.OrderItems = orderItems;
        return _mapper.Map<OrderResponseDto>(order);
    }

    private static string? ValidateOrderCreateDto(CreateOrderDto orderCreateDto)
    {
        if (orderCreateDto.Items == null || !orderCreateDto.Items.Any())
        {
            return "Order must contain at least one item.";
        }

        if (orderCreateDto.HasDiscount && (!orderCreateDto.DiscountValue.HasValue || orderCreateDto.DiscountValue.Value < 0))
        {
            return "A valid non-negative discount value is required when HasDiscount is true.";
        }

        return null;
    }

    private static decimal CalculateDiscount(bool hasDiscount, decimal? discountValue, decimal subtotal)
    {
        if (!hasDiscount)
        {
            return 0m;
        }

        var requestedDiscount = discountValue ?? 0m;
        return Math.Min(requestedDiscount, subtotal);
    }
}
