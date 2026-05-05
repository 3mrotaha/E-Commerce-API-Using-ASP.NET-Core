
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Interfaces;
using eCommerce.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the JWT token options from configuration
        services.Configure<JwtTokenOptions>(configuration.GetSection("JwtTokenOptions"));

        // Register the JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}