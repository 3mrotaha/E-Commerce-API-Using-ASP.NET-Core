namespace eCommerce.Application.DTOs.Auth;

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword
);
