namespace eCommerce.Application.DTOs.Auth;

public record AdminResponseDto(
    Guid Id,
    string FullName,
    string UserName,
    string Email,
    string? PhoneNumber,
    bool IsActivated,
    DateTime CreatedAt
);
