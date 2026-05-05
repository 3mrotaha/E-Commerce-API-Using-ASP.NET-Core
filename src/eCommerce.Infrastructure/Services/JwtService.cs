using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace eCommerce.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    public JwtTokenOptions JwtTokenOptions { get; init;}
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtTokenOptions> jwtTokenOptions, ILogger<JwtTokenService> logger)
    {
        _logger = logger;
        // Store the JWT token options for later use
        JwtTokenOptions = jwtTokenOptions.Value;
    }

    public AuthResponseDto GenerateToken(Account account, IEnumerable<string> roles)
    {

        if (!IsValidTokenOptions()){
            _logger.LogCritical("{Class}-{Method} null key found while generating JWT token", nameof(JwtTokenService), nameof(GenerateToken));
            throw new NullJwtSecretKeyException("Null JWT Secret Key Exception");
        }

        // sign in credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenOptions.SecretKey));
        var signedCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // build claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Email, account.Email ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiry = DateTime.UtcNow.AddMinutes(JwtTokenOptions.ExpiryMinutes);
        // create the token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtTokenOptions.Issuer,
            Audience = JwtTokenOptions.Audience,
            Expires = expiry,
            SigningCredentials = signedCredentials,
            Subject = new ClaimsIdentity(claims)
        };

        // generate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponseDto
        (
            AccessToken: tokenHandler.WriteToken(token),
            AccessTokenExpiry: expiry
        );
    }


    private bool IsValidTokenOptions()
    {
        return !(string.IsNullOrEmpty(JwtTokenOptions.SecretKey) || string.IsNullOrEmpty(JwtTokenOptions.Issuer)
                    || string.IsNullOrEmpty(JwtTokenOptions.Audience));
    }
}
