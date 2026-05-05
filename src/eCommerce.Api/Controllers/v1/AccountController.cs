using System.Security.Claims;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace eCommerce.Api.Controllers.V1
{
    public class AccountController : CustomControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }



        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("FixedWindowForLogin")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerData)
        {
            // validation
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            var result = await _authService.RegisterUserAsync(registerData);

            if (result.IsFailure)
                return ToActionResult(result);

            // assign Role
            if (result.Value!.responseData is not UserResponseDto userResponse)
                return BadRequest(new { Message = "Registration succeeded, but user response data is invalid." });

            var roleResult = await _authService.AddUserRole(userResponse.Id, Domain.Enums.AppUserRole.USER);
            if (roleResult.IsFailure)
                return ToActionResult(roleResult);

            return Ok(result.Value);
        }

        [HttpPost("register-admin")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SUPER_ADMIN")] // super admins only add admins
        [EnableRateLimiting("FixedWindowForLogin")]
        public async Task<IActionResult> RegisterAsAdmin([FromBody] RegisterUserDto registerUserDto)
        {
            // validation
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            var result = await _authService.RegisterUserAsync(registerUserDto, Domain.Enums.AppUserRole.ADMIN);

            if (result.IsFailure)
                return ToActionResult(result);

            // assign Role
            if (result.Value!.responseData is not AdminResponseDto userResponse)
                return BadRequest(new { Message = "Registration succeeded, but user response data is invalid." });

            var roleResult = await _authService.AddUserRole(userResponse.Id, Domain.Enums.AppUserRole.ADMIN);
            if (roleResult.IsFailure)
                return ToActionResult(roleResult);

            return Ok(result.Value);
        }

        [HttpPost("register-superadmin")]
        [EnableRateLimiting("FixedWindowForLogin")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SUPER_ADMIN")] // super admins only can add new super admins
        public async Task<IActionResult> RegisterAsSuperAdmin([FromBody] RegisterUserDto registerUserDto)
        {
            // validation
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            var result = await _authService.RegisterUserAsync(registerUserDto, Domain.Enums.AppUserRole.SUPER_ADMIN);

            if (result.IsFailure)
                return ToActionResult(result);

            // assign Role
            if (result.Value!.responseData is not SuperAdminResponseDto userResponse)
                return BadRequest(new { Message = "Registration succeeded, but user response data is invalid." });

            var roleResult = await _authService.AddUserRole(userResponse.Id, Domain.Enums.AppUserRole.SUPER_ADMIN);
            if (roleResult.IsFailure)
                return ToActionResult(roleResult);

            return Ok(result.Value);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("FixedWindowForLogin")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            var result = await _authService.LoginAsync(loginData);
            if (result.IsFailure)
                return Unauthorized(new { Message = "Invalid username or password" });

            return Ok(result.Value);
        }


        [HttpGet("login")]
        [AllowAnonymous]
        [EnableRateLimiting("FixedWindowForLogin")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var authResult = await HttpContext.AuthenticateAsync("Cookies");

            if (!authResult.Succeeded)
                return Unauthorized(new { Message = "Google login failed" });


            var emailClaim = authResult.Principal.FindFirstValue(ClaimTypes.Email);
            var nameClaim = authResult.Principal.FindFirstValue(ClaimTypes.Name);
            var openIdClaim = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (emailClaim is null)
                return BadRequest(new { Message = "Email claim not found in Google authentication response" });

            var email = emailClaim;
            var name = nameClaim ?? email.Substring(0, email.IndexOf('@')); // Use email as name if name claim is not available
            var openId = openIdClaim;

            if ((await _authService.FindUserByEmailAsync(email)).IsFailure)
            {
                // auto register the user if not exists
                var registerResult = await _authService.RegisterUserAsync(new RegisterUserDto(
                    FullName: name,
                    UserName: email.Substring(0, email.IndexOf('@')),
                    Email: email,
                    PhoneNumber: null, // generate a unique username based on email
                    Password: Guid.NewGuid().ToString() + "Abc!" // generate a random password that meets the complexity requirements
                ));

                if (registerResult.IsFailure)
                    return ToActionResult(registerResult);

                if (registerResult.Value!.responseData is not UserResponseDto)
                    return BadRequest(new { Message = "Google authentication succeeded, but automatic registration failed." });

                var roleResult = await _authService.AddUserRole(((UserResponseDto)registerResult.Value.responseData).Id, Domain.Enums.AppUserRole.USER);
                if (roleResult.IsFailure)
                    return ToActionResult(roleResult);
            }


            var loginResult = await _authService.LoginWithGoogleAsync(email);
            if (loginResult.IsFailure)
                return Unauthorized(new { Message = "Google login failed" });

            // return Redirect($"appp.com/callback#token={loginResult}");
            return Ok(loginResult.Value);
        }

        [HttpPut("update-account/{userId:guid:required}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> UpdateAccount([FromRoute] Guid userId, [FromBody] UpdateAccountDto updateData)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
                throw new UnautherizedException("You are not authorized to update this account.");

            var result = await _authService.UpdateAccountAsync(updateData, userId);
            return ToMessageActionResult(result, new { Message = "Account updated successfully" });
        }

        [HttpPost("change-password/{userId:guid:required}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> ChangePassword([FromRoute] Guid userId, [FromBody] ChangePasswordDto passData)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            if (!IsPrivilegedAccount() && !IsCurrentUser(userId))
                throw new UnautherizedException("You are not authorized to change this account password.");

            var result = await _authService.ChangePasswordAsync(passData, userId);
            return ToMessageActionResult(result, new { Message = "Password changed successfully" });
        }

        [HttpPost("refresh-token/{userId:guid:required}")]
        [AllowAnonymous]
        [EnableRateLimiting("FixedWindowForLogin")]
        public async Task<IActionResult> RefreshToken([FromRoute] Guid userId, [FromBody] AuthResponseDto refreshTokenData)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                                                                            .Select(e => e.ErrorMessage)
                                                                            .ToList());

            var result = await _authService.RefreshTokenAsync(refreshTokenData, userId);
            if (result.IsFailure)
                return Unauthorized(new { Message = "Invalid refresh token" });

            return Ok(result.Value);
        }


        [HttpGet("logout")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPut("activate/admin/{adminId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "SUPER_ADMIN")] // super admins only activate admins accounts
        public async Task<IActionResult> ActivateAdminAccount([FromRoute] Guid adminId)
        {
            var result = await _authService.ActivateAdminAccount(adminId);
            return ToMessageActionResult(result, "Admin Account Activated Successfully");
        }
    }
}
