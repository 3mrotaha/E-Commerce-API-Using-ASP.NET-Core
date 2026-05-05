namespace eCommerce.Application.DTOs.PaymentRecord;

public record PaymentRecordResponseDto(
    Guid Id,
    Guid OrderId,
    Guid? PaymentMethodId,
    string? MaskedCardNumber,
    decimal Amount,
    DateTime CreatedAt
);
