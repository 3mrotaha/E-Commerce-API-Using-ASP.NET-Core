using eCommerce.Application.DTOs.PaymentRecord;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<IEnumerable<PaymentRecordResponseDto>>> GetPaymentsByUserAsync(Guid userId);
    Task<Result<PaymentRecordResponseDto>> GetPaymentByIdAsync(Guid paymentId, Guid userId);
    Task<Result<PaymentRecordResponseDto>> GetPaymentByOrderIdAsync(Guid orderId, Guid userId);
}
