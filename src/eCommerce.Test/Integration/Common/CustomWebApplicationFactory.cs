using System.Threading.RateLimiting;
using eCommerce.Persistence.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace eCommerce.Test.Integration.Common;

/// <summary>
/// Boots the real API in-process for integration testing, but replaces the few infrastructure
/// dependencies that would otherwise require external services (SQL Server, Redis) or get in the way
/// of testing (JWT signing, rate limiting).
/// <para>
/// One factory owns one in-memory database (unique name per instance), so different test classes are
/// isolated from each other. Tests share a factory within a class via <c>IClassFixture</c>; to stay
/// independent they operate on their own freshly-generated user ids rather than shared mutable state.
/// </para>
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // A unique database name keeps each factory's data separate from every other factory.
    private readonly string _databaseName = $"ecommerce-tests-{Guid.NewGuid()}";

    public CustomWebApplicationFactory()
    {
        // The API reads several settings *eagerly* during service registration (AddApiServices binds
        // JwtTokenOptions and would throw without it; AddGoogle reads the client id/secret). With the
        // minimal-hosting model, ConfigureAppConfiguration callbacks are applied too late — after the
        // app has already built its configuration. Environment variables, by contrast, are picked up by
        // WebApplication.CreateBuilder at the very start, so they are visible to that early code.
        // ("__" is the configuration hierarchy separator for environment variables.)
        SetEnv("JwtTokenOptions__SecretKey", "integration-tests-super-secret-signing-key-0123456789");
        SetEnv("JwtTokenOptions__Issuer", "ecommerce-tests");
        SetEnv("JwtTokenOptions__Audience", "ecommerce-tests");
        SetEnv("JwtTokenOptions__ExpiryMinutes", "60");
        SetEnv("JwtTokenOptions__RefreshTokenExpiryMinutes", "10080");
        SetEnv("Authentication__Google__ClientId", "test-google-client-id");
        SetEnv("Authentication__Google__ClientSecret", "test-google-client-secret");
        SetEnv("ConnectionStrings__DefaultConnectionString", "InMemory-Not-Used");
        SetEnv("ConnectionStrings__Redis", "localhost:6379");
    }

    private static void SetEnv(string key, string value) => Environment.SetEnvironmentVariable(key, value);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" environment skips the Development-only Swagger branch and the appsettings.Development
        // sinks (SQL Server / Seq logging), keeping the test host self-contained.
        builder.UseEnvironment("Testing");

        // ConfigureTestServices runs after the app's own registrations, so these calls override production wiring.
        builder.ConfigureTestServices(services =>
        {
            ReplaceDatabaseWithInMemory(services);
            ReplaceRedisWithInMemoryCache(services);
            OverrideBearerSchemeWithTestHandler(services);
            DisableRateLimiting(services);
        });
    }

    /// <summary>
    /// Swaps the SQL Server provider for the EF Core in-memory provider. The production registration
    /// leaves several option services behind; all must be removed or EF complains that two providers
    /// are configured at once.
    /// </summary>
    private void ReplaceDatabaseWithInMemory(IServiceCollection services)
    {
        var optionsDescriptors = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                // EF Core 9+ also registers IDbContextOptionsConfiguration<AppDbContext>.
                (d.ServiceType.IsGenericType && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")))
            .ToList();

        foreach (var descriptor in optionsDescriptors)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseInMemoryDatabase(_databaseName)
                // The in-memory provider can't honour transactions; silence the warning repositories trigger.
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
    }

    /// <summary>
    /// Replaces the Redis-backed distributed cache (and connection multiplexer) with an in-memory cache,
    /// so tests need no running Redis instance.
    /// </summary>
    private static void ReplaceRedisWithInMemoryCache(IServiceCollection services)
    {
        services.RemoveAll<IDistributedCache>();
        services.AddDistributedMemoryCache();

        // The multiplexer is registered as a singleton that connects to Redis; a mock prevents any connect attempt.
        services.RemoveAll<IConnectionMultiplexer>();
        services.AddSingleton(Mock.Of<IConnectionMultiplexer>());
    }

    /// <summary>
    /// Re-points the existing "Bearer" authentication scheme at <see cref="TestAuthHandler"/>. Every
    /// controller authorizes against "Bearer" explicitly, and it is also the default scheme, so this one
    /// change makes header-based test identities flow through the entire authorization pipeline.
    /// </summary>
    private static void OverrideBearerSchemeWithTestHandler(IServiceCollection services)
    {
        services.AddTransient<TestAuthHandler>();
        services.Configure<AuthenticationOptions>(options =>
        {
            if (options.SchemeMap.TryGetValue("Bearer", out var bearer))
            {
                bearer.HandlerType = typeof(TestAuthHandler);
            }
        });
    }

    /// <summary>
    /// Neutralises the strict login rate limiter (5 requests/minute) which would otherwise make the
    /// account tests flaky. The production options are removed and the referenced policy is re-declared
    /// as a no-op limiter.
    /// </summary>
    private static void DisableRateLimiting(IServiceCollection services)
    {
        // Drop the production rate-limiter configuration (the strict 5/min login policy lives here).
        // The core rate-limiter services registered by AddRateLimiter are not IConfigureOptions, so they
        // stay in place; only the policy definitions are replaced below.
        services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
        services.RemoveAll<IPostConfigureOptions<RateLimiterOptions>>();

        services.Configure<RateLimiterOptions>(options =>
        {
            // The login endpoints reference this policy by name; GetNoLimiter lets every request through.
            options.AddPolicy("FixedWindowForLogin", _ => RateLimitPartition.GetNoLimiter("test"));
        });
    }

    /// <summary>
    /// Creates the in-memory database and applies the HasData seed rows (categories, products, reviews,
    /// users). Safe to call repeatedly — EnsureCreated is a no-op once the database exists.
    /// </summary>
    public void EnsureDatabaseSeeded()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
}
