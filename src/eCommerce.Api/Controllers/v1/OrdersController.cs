using eCommerce.Application.DTOs.Order;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    public class OrdersController : CustomControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetOrdersByUser([FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return ToActionResult(orders);
        }

        [HttpGet("{orderId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId, [FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            return ToActionResult(order);
        }

        [HttpPost("user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> CreateOrder([FromRoute] Guid userId, [FromBody] CreateOrderDto orderCreateDto)
        {
            EnsureCanAccessUserScope(userId);

            var createdOrder = await _orderService.CreateOrderAsync(userId, orderCreateDto);
            return ToCreatedAtActionResult(createdOrder, nameof(GetOrderById), new { orderId = createdOrder.Value?.Id, userId });
        }

        [HttpPut("{orderId:guid}/state")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> UpdateOrderState([FromRoute] Guid orderId, [FromBody] UpdateOrderStateDto orderStateDto)
        {
            var updatedOrder = await _orderService.UpdateOrderStateAsync(orderId, orderStateDto);
            return ToActionResult(updatedOrder);
        }

        [HttpDelete("{orderId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> DeleteOrder([FromRoute] Guid orderId)
        {
            var isDeleted = await _orderService.DeleteOrderAsync(orderId);
            return ToMessageActionResult(isDeleted, new { Message = $"Order with id={orderId} deleted successfully" });
        }

        private void EnsureCanAccessUserScope(Guid userId)
        {
            if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to access this user's orders.");
            }
        }
    }
}
