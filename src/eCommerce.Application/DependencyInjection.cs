using eCommerce.Application.Interfaces;
using eCommerce.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace eCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductReviewService, ProductReviewService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        var RedisConnectionString = configuration.GetConnectionString("Redis");
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = RedisConnectionString;
            options.InstanceName = "ECommerceApp:";
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
           return ConnectionMultiplexer.Connect(RedisConnectionString!);
        });

        return services;
    }
}
