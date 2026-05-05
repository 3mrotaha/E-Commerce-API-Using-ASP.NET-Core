using eCommerce.Application.DTOs.CartItem;

namespace eCommerce.Application.DTOs.Cart;

public record CartResponseDto(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemResponseDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
