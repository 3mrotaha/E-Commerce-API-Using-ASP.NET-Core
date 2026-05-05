using System.Diagnostics;
using System.Security.Claims;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eCommerce.Api.Middlewares;

public class AuthorizeAdminAccountActivated
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizeAdminAccountActivated> _logger;

    public AuthorizeAdminAccountActivated(RequestDelegate next, ILogger<AuthorizeAdminAccountActivated> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        _logger.LogDebug("Executing middleware {Middleware}", nameof(AuthorizeAdminAccountActivated));
        if (context.User.IsInRole("ADMIN"))
        {
            var accountEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if(accountEmail is null)
                throw new UnautherizedException("Admin email can't be null");
            
            var isActivated = await authService.IsAdminActivated(accountEmail);
            if(isActivated.IsFailure || isActivated.Value == false)
                throw new UnautherizedException("Admin account isn't activated");
        }

        await _next(context);

        // after next middleware
    }
}

public static class AuthorizeAdminAccountActivatedExtensions
{
    public static IApplicationBuilder UseAuthorizeAdminAccountActivated(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizeAdminAccountActivated>();
    }
}
