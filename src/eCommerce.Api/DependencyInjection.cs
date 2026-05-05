using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using eCommerce.Api.Filters;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Identity;
using eCommerce.Persistence.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

namespace eCommerce.Api;

public static class ApiDependencyInjectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers(options =>
        {
            /* Adding the ProducesAttribute and ConsumesAttribute globally to all the controllers and actions to specify that the API will only accept and return JSON data */
            options.Filters.Add(new ProducesAttribute("application/json", "application/pdf"));
            options.Filters.Add(new ConsumesAttribute("application/json"));
            options.Filters.Add<StructuredActionLoggingFilter>();

            /* Adding the AuthorizeFilter globally to all the controllers and actions to require authentication for all the endpoints by default, this means that all the endpoints will require authentication unless they are decorated with the [AllowAnonymous] attribute */
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                            .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429; // Too Many Requests

            // Limit login attempts to 5 per minute per IP address
            options.AddPolicy("FixedWindowForLogin", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                 partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                 factory: _ => new FixedWindowRateLimiterOptions
                 {
                     PermitLimit = 5,
                     Window = TimeSpan.FromMinutes(1),
                     QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                     QueueLimit = 0
                 }
                );
            });

            options.AddSlidingWindowLimiter("SlidingWindowLimiter", options =>
            {
                options.SegmentsPerWindow = 10;
                options.PermitLimit = 100;
                options.AutoReplenishment = true;
                options.Window = TimeSpan.FromSeconds(30);
                options.QueueLimit = 1000;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.AddConcurrencyLimiter("ConcurrentRequestsLimiter", options =>
            {
                options.PermitLimit = 10;
                options.QueueLimit = 100;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        services.AddIdentity<Account, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<Account, ApplicationRole, AppDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, AppDbContext, Guid>>();

        var jwt = configuration.GetSection("JwtTokenOptions").Get<JwtTokenOptions>() ?? throw new NullJwtSecretKeyException("JwtTokenOptions is not configured.");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie("Cookies")
        .AddGoogle("Google", options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is not configured.");
            options.SignInScheme = "Cookies";
            // request addtional user information from Google
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("openid");
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine(context.Exception.ToString());
                    return Task.CompletedTask;
                }
            };
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
            };

        });

        services.AddAuthorization();

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            options.RequestBodyLogLimit = 4096;
            options.ResponseBodyLogLimit = 4096;
        });

        return services;
    }
    public static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
        }).AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "E-Commerce Api v1.0",
                Version = "1.0",

            });

            var SecuritySchema = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer",
                In = ParameterLocation.Header,
                Description = "Enter Jwt Token like: Beare {token}"
            };

            options.AddSecurityDefinition("Bearer", SecuritySchema);

            options.AddSecurityRequirement(document =>
            {
                return new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                };
            });


        });



        return services;
    }
}
