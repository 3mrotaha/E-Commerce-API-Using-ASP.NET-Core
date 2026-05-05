namespace eCommerce.Application.DTOs.Auth;

public record SuperAdminResponseDto(
    Guid Id,
    string FullName,
    string UserName,
    string Email,
    string? PhoneNumber,
    DateTime CreatedAt
);
