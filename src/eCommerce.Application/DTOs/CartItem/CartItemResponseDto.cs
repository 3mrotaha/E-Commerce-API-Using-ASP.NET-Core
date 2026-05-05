namespace eCommerce.Application.DTOs.CartItem;

public record CartItemResponseDto(
    int Id,
    Guid CartId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime UpdatedAt
);
