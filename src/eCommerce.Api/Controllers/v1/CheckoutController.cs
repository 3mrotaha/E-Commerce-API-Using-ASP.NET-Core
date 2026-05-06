using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1;

public class CheckoutController : CustomControllerBase
{
    private readonly ILogger<CheckoutController> _logger;
    private readonly ICartService _cartService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IOrderService _orderService;
    public CheckoutController(ICartService cartService, IPaymentMethodService paymentMethodService, IOrderService orderService, ILogger<CheckoutController> logger)
    {
        _logger = logger;
        _paymentMethodService = paymentMethodService;
        _cartService = cartService;
        _orderService = orderService;
    }

    [HttpPost("user/{userId:guid}")]
    public async Task<IActionResult> Checkout([FromRoute] Guid userId)
    {
        var userCart = await _cartService.GetCartByUserAsync(userId);
        if(userCart.IsFailure)
        {
            return ToActionResult(userCart);
        }

        var paymentMethods = await _paymentMethodService.GetCardPaymentMethodsAsync(userId);

        if(paymentMethods.IsFailure || !paymentMethods.Value!.Any())
        {
            return ToActionResult(paymentMethods);
        }

        // order create request
        var orderCreateReq = new CreateOrderDto(
            Items: userCart.Value!.Items.Select(i => new CreateOrderItemDto(i.ProductId, i.Quantity)).ToList(),
            HasDiscount: false,
            DiscountValue: 0,
            PaymentMethodId: paymentMethods.Value?.FirstOrDefault()?.Id ?? Guid.Empty
        );

        var createdOrder = await _orderService.CreateOrderAsync(userId, orderCreateReq);

        // clear cart        
        await _cartService.EmptyCartAsync(userId);

        // payment processing
        // TODO

        return ToCreatedAtActionResult(createdOrder, nameof(OrdersController.GetOrderById), nameof(OrdersController).Replace("Controller", ""), new { orderId = createdOrder.Value?.Id, userId });
    }
}
