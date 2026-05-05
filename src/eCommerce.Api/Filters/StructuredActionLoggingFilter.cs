using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eCommerce.Api.Filters;

public class StructuredActionLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<StructuredActionLoggingFilter> _logger;

    public StructuredActionLoggingFilter(ILogger<StructuredActionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var controllerName = actionDescriptor?.ControllerName ?? context.RouteData.Values["controller"]?.ToString();
        var actionName = actionDescriptor?.ActionName ?? context.RouteData.Values["action"]?.ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["Controller"] = controllerName,
            ["Action"] = actionName,
            ["TraceIdentifier"] = context.HttpContext.TraceIdentifier,
            ["RequestPath"] = context.HttpContext.Request.Path.Value,
            ["RequestMethod"] = context.HttpContext.Request.Method
        });

        _logger.LogDebug(
            "Executing controller action {Controller}.{Action} with route values {@RouteValues}",
            controllerName,
            actionName,
            context.RouteData.Values);

        ActionExecutedContext executedContext;
        try
        {
            executedContext = await next();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Unhandled exception while executing controller action {Controller}.{Action}",
                controllerName,
                actionName);
            throw;
        }

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            _logger.LogCritical(
                executedContext.Exception,
                "Unhandled exception returned from controller action {Controller}.{Action}",
                controllerName,
                actionName);
            return;
        }

        var statusCode = context.HttpContext.Response.StatusCode;
        if (statusCode >= StatusCodes.Status400BadRequest)
        {
            _logger.LogError(
                "Controller action {Controller}.{Action} completed with status code {StatusCode}",
                controllerName,
                actionName,
                statusCode);
            return;
        }

        _logger.LogInformation(
            "Controller action {Controller}.{Action} completed with status code {StatusCode}",
            controllerName,
            actionName,
            statusCode);
    }
}
