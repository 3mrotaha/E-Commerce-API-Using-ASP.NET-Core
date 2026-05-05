namespace eCommerce.Application.DTOs.Auth;

public class JwtTokenOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 10; // Default to 10 minutes
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int RefreshTokenExpiryMinutes { get; set; } = 7 * 24 * 60; // Default to 7 days
}