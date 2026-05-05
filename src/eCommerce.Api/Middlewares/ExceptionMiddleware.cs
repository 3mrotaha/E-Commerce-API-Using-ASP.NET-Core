using System.Net;
using System.Text.Json;
using eCommerce.Application.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eCommerce.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // before next middleware
        try
        {
            _logger.LogDebug("Executing middleware {Middleware}", nameof(ExceptionMiddleware));
            await _next(context);
        }
        catch(Exception ex)
        {
            await HandleException(context, ex, _logger);
        }
    }

    public static async Task HandleException(HttpContext context, Exception ex, ILogger<ExceptionMiddleware> logger)
    {
        context.Response.ContentType = "application/json";
        (int statusCode, object body) = ex switch
        {
            BadRequestException badRequest => (
                (int) StatusCodes.Status400BadRequest,
                (object) new { message = badRequest.Message, errors = badRequest.Errors}
            ),
            UnautherizedException unautherized => (
                (int) StatusCodes.Status401Unauthorized,
                (object) new { message = unautherized.Message }
            ),
            NotFoundException notFound => (
                (int) StatusCodes.Status404NotFound,
                (object) new {message = notFound.Message}
            ),
            _ => (
                (int) StatusCodes.Status500InternalServerError,
                (object) new {message = "An unexpected error occurred"}
            )            
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogCritical(
                ex,
                "Unhandled exception returned {StatusCode} for {RequestMethod} {RequestPath}",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            logger.LogError(
                ex,
                "Handled application exception returned {StatusCode} for {RequestMethod} {RequestPath}",
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(body, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}
