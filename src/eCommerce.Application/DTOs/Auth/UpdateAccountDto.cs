namespace eCommerce.Application.DTOs.Auth;

public record UpdateAccountDto(
    string FullName,
    string? PhoneNumber
);
