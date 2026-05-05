using eCommerce.Application.DTOs.OrderItem;

namespace eCommerce.Application.DTOs.Order;

public record CreateOrderDto(
    Guid PaymentMethodId,
    IEnumerable<CreateOrderItemDto> Items,
    bool HasDiscount,
    decimal? DiscountValue
);
