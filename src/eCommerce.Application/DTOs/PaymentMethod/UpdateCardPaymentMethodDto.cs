namespace eCommerce.Application.DTOs.PaymentMethod;

public record UpdateCardPaymentMethodDto(
    string CardHolderName,
    DateTime CardExpiryDate
);
