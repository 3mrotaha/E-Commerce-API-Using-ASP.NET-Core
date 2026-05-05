namespace eCommerce.Application.DTOs.CartItem;

public record AddCartItemDto(
    Guid ProductId,
    int Quantity
);
