using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    public class PaymentsController : CustomControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetPaymentsByUser([FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var payments = await _paymentService.GetPaymentsByUserAsync(userId);
            return ToActionResult(payments);
        }

        [HttpGet("{paymentId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetPaymentById([FromRoute] Guid paymentId, [FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId, userId);
            return ToActionResult(payment);
        }

        [HttpGet("order/{orderId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetPaymentByOrderId([FromRoute] Guid orderId, [FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId, userId);
            return ToActionResult(payment);
        }

        private void EnsureCanAccessUserScope(Guid userId)
        {
            if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to access this user's payments.");
            }
        }
    }
}
