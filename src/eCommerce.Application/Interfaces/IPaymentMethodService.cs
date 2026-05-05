using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IPaymentMethodService
{
    Task<Result<IEnumerable<CardPaymentMethodResponseDto>>> GetCardPaymentMethodsAsync(Guid userId);
    Task<Result<CardPaymentMethodResponseDto>> GetCardPaymentMethodByIdAsync(Guid paymentMethodId, Guid userId);
    Task<Result<CardPaymentMethodResponseDto>> AddCardPaymentMethodAsync(Guid userId, AddCardPaymentMethodDto addCardPaymentMethodDto);
    Task<Result<CardPaymentMethodResponseDto>> UpdateCardPaymentMethodAsync(Guid paymentMethodId, Guid userId, UpdateCardPaymentMethodDto updateCardPaymentMethodDto);
    Task<Result<bool>> DeleteCardPaymentMethodAsync(Guid paymentMethodId, Guid userId);
}
