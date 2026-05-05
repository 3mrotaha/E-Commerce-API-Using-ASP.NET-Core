namespace eCommerce.Application.DTOs.Auth;

public record RegisterResultDto(
    bool Succeeded,
    List<string> Errors,
    object? responseData = default
);
