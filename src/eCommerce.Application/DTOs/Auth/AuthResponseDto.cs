namespace eCommerce.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiry,
    Guid? UserId = default,
    string? RefreshToken = default,
    DateTime? RefreshTokenExpiry = default
);
