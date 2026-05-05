namespace eCommerce.Application.DTOs.Auth;

public record RegisterUserDto(
    string FullName,
    string UserName,
    string Email,
    string Password,
    string? PhoneNumber
);
