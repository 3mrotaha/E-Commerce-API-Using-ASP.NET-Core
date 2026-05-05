namespace eCommerce.Application.DTOs.PaymentMethod;

public record CardPaymentMethodResponseDto(
    Guid Id,
    Guid UserId,
    string CardHolderName,
    string MaskedCardNumber,
    DateTime CardExpiryDate,
    DateTime UpdatedAt
);
