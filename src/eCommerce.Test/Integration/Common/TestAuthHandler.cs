using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eCommerce.Test.Integration.Common;

/// <summary>
/// A fake authentication handler used only in integration tests.
/// <para>
/// The real API authenticates with JWT bearer tokens. Producing and signing real tokens for every
/// test (and satisfying issuer/audience/lifetime validation) is noisy, so instead this handler reads
/// the caller's desired identity straight from request headers and turns it into a <see cref="ClaimsPrincipal"/>.
/// </para>
/// <para>
/// Every controller in the API declares <c>[Authorize(AuthenticationSchemes = "Bearer")]</c>, so the
/// test host re-points the existing "Bearer" scheme at this handler (see CustomWebApplicationFactory).
/// That means the production authorization rules (roles, ownership checks) still run unchanged — only
/// the token-validation step is swapped out.
/// </para>
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Header names a test sets to describe "who" is calling. They are arbitrary and test-only.
    public const string UserIdHeader = "X-Test-UserId";
    public const string EmailHeader = "X-Test-Email";
    public const string RolesHeader = "X-Test-Roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // No identity header => treat the request as anonymous. Returning NoResult (rather than Fail)
        // lets the authorization layer issue a clean 401 challenge, exactly like a missing JWT would.
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId) || string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Build the same claim set the production JWT carries: the user id, the email, and the roles.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId!),
        };

        if (Request.Headers.TryGetValue(EmailHeader, out var email) && !string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email!));
        }

        if (Request.Headers.TryGetValue(RolesHeader, out var roles) && !string.IsNullOrWhiteSpace(roles))
        {
            // Roles arrive as a comma-separated list; each becomes its own role claim so IsInRole works.
            foreach (var role in roles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
