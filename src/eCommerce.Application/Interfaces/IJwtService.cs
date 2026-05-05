using eCommerce.Domain.Identity;
using eCommerce.Application.DTOs.Auth;

namespace eCommerce.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenOptions JwtTokenOptions { get; init; }
    AuthResponseDto GenerateToken(Account account, IEnumerable<string> roles);
}