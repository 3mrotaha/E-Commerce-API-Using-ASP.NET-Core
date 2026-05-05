using eCommerce.Application.DTOs.Order;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result<IEnumerable<OrderResponseDto>>> GetOrdersByUserAsync(Guid userId);
    Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid userId);
    Task<Result<OrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderDto orderCreateDto);
    Task<Result<OrderResponseDto>> UpdateOrderStateAsync(Guid orderId, UpdateOrderStateDto orderStateDto);
    Task<Result<bool>> DeleteOrderAsync(Guid orderId);
}
