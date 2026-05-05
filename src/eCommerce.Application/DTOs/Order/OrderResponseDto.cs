using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Domain.Enums;

namespace eCommerce.Application.DTOs.Order;

public record OrderResponseDto(
    Guid Id,
    Guid UserId,
    Guid PaymentId,
    OrderState OrderState,
    bool HasDiscount,
    decimal? DiscountValue,
    IEnumerable<OrderItemResponseDto> Items,
    DateTime CreatedAt
);
