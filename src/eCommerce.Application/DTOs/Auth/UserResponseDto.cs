namespace eCommerce.Application.DTOs.Auth;

public record UserResponseDto(
    Guid Id,
    string FullName,
    string? UserName,
    string Email,
    string? PhoneNumber,
    bool IsVipUser,
    int Points,
    DateTime CreatedAt
);
