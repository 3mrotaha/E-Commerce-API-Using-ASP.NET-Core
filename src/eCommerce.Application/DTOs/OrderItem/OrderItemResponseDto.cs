namespace eCommerce.Application.DTOs.OrderItem;

public record OrderItemResponseDto(
    int Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
