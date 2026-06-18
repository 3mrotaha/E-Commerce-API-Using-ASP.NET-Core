using System.IdentityModel.Tokens.Jwt;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Exceptions;
using eCommerce.Domain.Identity;
using eCommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace eCommerce.Test.Infrastructure;

/// <summary>
/// Tests <see cref="JwtTokenService"/>. No mocks needed: <c>IOptions</c> and <c>ILogger</c> are
/// supplied with real lightweight instances (<see cref="Options.Create{T}"/> / <see cref="NullLogger{T}"/>).
/// </summary>
public class JwtTokenService_Test
{
    private static JwtTokenOptions ValidOptions() => new()
    {
        // HmacSha256 requires a key of at least 256 bits (32 bytes).
        SecretKey = "super-secret-signing-key-of-sufficient-length-1234567890",
        Issuer = "ecommerce-tests",
        Audience = "ecommerce-clients",
        ExpiryMinutes = 30
    };

    private static JwtTokenService CreateService(JwtTokenOptions options) =>
        new(Options.Create(options), NullLogger<JwtTokenService>.Instance);

    [Fact]
    public void GenerateToken_WhenOptionsValid_ShouldEmitTokenWithExpectedClaims()
    {
        var options = ValidOptions();
        var service = CreateService(options);
        var account = new User { Id = Guid.NewGuid(), Email = "user@example.com" };

        var response = service.GenerateToken(account, new[] { "USER", "ADMIN" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        jwt.Issuer.Should().Be(options.Issuer);
        jwt.Audiences.Should().Contain(options.Audience);
        // The handler emits claims under the short JWT names (nameid/email/role), not the long URIs.
        jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == account.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == account.Email);
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value)
            .Should().BeEquivalentTo(new[] { "USER", "ADMIN" });
    }

    [Fact]
    public void GenerateToken_WhenOptionsValid_ShouldSetFutureExpiry()
    {
        var service = CreateService(ValidOptions());

        var response = service.GenerateToken(new User { Id = Guid.NewGuid(), Email = "u@e.com" }, Array.Empty<string>());

        response.AccessTokenExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Theory]
    [InlineData("", "iss", "aud")]
    [InlineData("super-secret-signing-key-of-sufficient-length-1234567890", "", "aud")]
    [InlineData("super-secret-signing-key-of-sufficient-length-1234567890", "iss", "")]
    public void GenerateToken_WhenRequiredOptionMissing_ShouldThrowNullJwtSecretKeyException(
        string secret, string issuer, string audience)
    {
        var service = CreateService(new JwtTokenOptions { SecretKey = secret, Issuer = issuer, Audience = audience });

        var act = () => service.GenerateToken(new User { Id = Guid.NewGuid(), Email = "u@e.com" }, Array.Empty<string>());

        act.Should().Throw<NullJwtSecretKeyException>();
    }
}
