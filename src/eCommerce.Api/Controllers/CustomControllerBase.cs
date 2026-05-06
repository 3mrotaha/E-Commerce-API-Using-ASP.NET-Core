using Asp.Versioning;
using eCommerce.Application.Common;
using eCommerce.Application.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    public class CustomControllerBase : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnautherizedException("Invalid or missing user identifier in token.");
            }

            return parsedUserId;
        }

        protected bool IsCurrentUser(Guid userId)
        {
            return GetCurrentUserId() == userId;
        }

        protected bool IsPrivilegedAccount()
        {
            return User.IsInRole("ADMIN") || User.IsInRole("SUPER_ADMIN");
        }

        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return ToFailureActionResult(result);
        }

        protected IActionResult ToCreatedAtActionResult<T>(Result<T> result, string actionName, object? routeValues)
        {
            if (result.IsSuccess)
            {
                return CreatedAtAction(actionName, routeValues, result.Value);
            }

            return ToFailureActionResult(result);
        }

        protected IActionResult ToCreatedAtActionResult<T>(Result<T> result, string actionName, string controllerName, object? routeValues)
        {
            if (result.IsSuccess)
            {
                return CreatedAtAction(actionName, controllerName, routeValues, result.Value);
            }

            return ToFailureActionResult(result);
        }

        protected IActionResult ToMessageActionResult(Result<bool> result, object successBody)
        {
            if (result.IsSuccess)
            {
                return Ok(successBody);
            }

            return ToFailureActionResult(result);
        }

        private ObjectResult ToFailureActionResult<T>(Result<T> result)
        {
            var error = result.Error ?? new Error("failure", "An unexpected service failure occurred", ErrorType.Failure);
            var body = error.Errors is { Count: > 0 }
                ? new { message = error.Message, errors = error.Errors }
                : (object)new { message = error.Message };

            return error.Type switch
            {
                ErrorType.NotFound => NotFound(body),
                ErrorType.Validation or ErrorType.Conflict => BadRequest(body),
                _ => StatusCode(StatusCodes.Status500InternalServerError, body)
            };
        }
    }
}
