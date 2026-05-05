using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    public class PaymentMethodsController : CustomControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodsController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpGet("user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetPaymentMethodsByUser([FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var paymentMethods = await _paymentMethodService.GetCardPaymentMethodsAsync(userId);
            return ToActionResult(paymentMethods);
        }

        [HttpGet("{paymentMethodId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetPaymentMethodById([FromRoute] Guid paymentMethodId, [FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var paymentMethod = await _paymentMethodService.GetCardPaymentMethodByIdAsync(paymentMethodId, userId);
            return ToActionResult(paymentMethod);
        }

        [HttpPost("user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> AddPaymentMethod([FromRoute] Guid userId, [FromBody] AddCardPaymentMethodDto addCardPaymentMethodDto)
        {
            EnsureCanAccessUserScope(userId);

            var paymentMethod = await _paymentMethodService.AddCardPaymentMethodAsync(userId, addCardPaymentMethodDto);
            return ToCreatedAtActionResult(paymentMethod, nameof(GetPaymentMethodById), new { paymentMethodId = paymentMethod.Value?.Id, userId });
        }

        [HttpPut("{paymentMethodId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> UpdatePaymentMethod([FromRoute] Guid paymentMethodId, [FromRoute] Guid userId, [FromBody] UpdateCardPaymentMethodDto updateCardPaymentMethodDto)
        {
            EnsureCanAccessUserScope(userId);

            var paymentMethod = await _paymentMethodService.UpdateCardPaymentMethodAsync(paymentMethodId, userId, updateCardPaymentMethodDto);
            return ToActionResult(paymentMethod);
        }

        [HttpDelete("{paymentMethodId:guid}/user/{userId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> DeletePaymentMethod([FromRoute] Guid paymentMethodId, [FromRoute] Guid userId)
        {
            EnsureCanAccessUserScope(userId);

            var isDeleted = await _paymentMethodService.DeleteCardPaymentMethodAsync(paymentMethodId, userId);
            return ToMessageActionResult(isDeleted, new { Message = $"Payment method with id={paymentMethodId} deleted successfully" });
        }

        private void EnsureCanAccessUserScope(Guid userId)
        {
            if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to access this user's payment methods.");
            }
        }
    }
}
