namespace eCommerce.Application.DTOs.PaymentMethod;

public record AddCardPaymentMethodDto(
    string CardNumber,
    DateTime CardExpiryDate,
    string CVV,
    string CardHolderName
);
