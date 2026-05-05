namespace eCommerce.Application.DTOs.OrderItem;

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity
);
