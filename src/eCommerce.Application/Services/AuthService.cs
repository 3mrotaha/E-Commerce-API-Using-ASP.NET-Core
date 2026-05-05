using System.Security.Cryptography;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Enums;
using eCommerce.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Account> _appUserManager;
    private readonly RoleManager<ApplicationRole> _appRoleManager;
    private readonly SignInManager<Account> _userSignInManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    public AuthService(UserManager<Account> appUserManager,
                    RoleManager<ApplicationRole> appRoleManage,
                    SignInManager<Account> userSignInManager,
                    IMapper mapper,
                    ILogger<AuthService> logger,
                    IJwtTokenService jwtTokenService)
    {
        _appUserManager = appUserManager;
        _appRoleManager = appRoleManage;
        _userSignInManager = userSignInManager;
        _mapper = mapper;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<bool>> ActivateAdminAccount(Guid adminId)
    {
        _logger.LogDebug("Activating admin account {AdminId}", adminId);

        var account = await _appUserManager.Users.OfType<Admin>()
                                        .FirstOrDefaultAsync(a => a.Id == adminId);

        if(account == null)
        {
            _logger.LogError("Cannot activate admin account {AdminId}; account was not found", adminId);
            return Result<bool>.NotFound($"Couldn't find Admin account of id={adminId}");
        }
        
        account.IsActivated = true;

        var result = await _appUserManager.UpdateAsync(account);
        
        if(result.Succeeded == false)
        {
            _logger.LogError("Failed to activate admin account {AdminId} due to {Errors}", adminId, result.Errors);
            return Result<bool>.BadRequest($"Couldn't activate admin account of id={adminId}");
        }

        _logger.LogInformation("Activated admin account {AdminId}", adminId);
        return Result<bool>.Success(true);
    }


    public async Task<Result<bool>> AddUserRole(Guid userId, AppUserRole role)
    {      
        _logger.LogDebug("Adding role {Role} to user {UserId}", role, userId);

        if (await _appRoleManager.RoleExistsAsync(role.ToString()) == false)
        {
            // create role if not exists
            var roleResult = await _appRoleManager.CreateAsync(new ApplicationRole() { Name = role.ToString() });
            if (roleResult.Succeeded == false)
            {
                _logger.LogDebug("Couldn't create role {Role} Due to {Errors}", role, roleResult.Errors);
                return Result<bool>.Validation(
                    $"Couldn't create role {role}",
                    roleResult.Errors.Select(e => e.Description).ToList());
            }
        }


        Account? account = await _appUserManager.FindByIdAsync(userId.ToString());

        if (account == null)
        {
            _logger.LogError("Cannot add role {Role}; account {UserId} was not found", role, userId);
            return Result<bool>.NotFound($"Couldn't find account of id={userId}");
        }

        IdentityResult result = role switch
        {
            AppUserRole.ADMIN => await _appUserManager.AddToRoleAsync((account as Admin)!, AppUserRole.ADMIN.ToString()),
            AppUserRole.SUPER_ADMIN => await _appUserManager.AddToRoleAsync((account as SuperAdmin)!, AppUserRole.SUPER_ADMIN.ToString()),
            _ => await _appUserManager.AddToRoleAsync((account as User)!, AppUserRole.USER.ToString())
        };

        if (result.Succeeded)
        {
            _logger.LogInformation("Added role {Role} to user {UserId}", role, userId);
            return Result<bool>.Success(true);
        }

        _logger.LogError("Failed to add role {Role} to user {UserId} due to {Errors}", role, userId, result.Errors);
        return Result<bool>.Validation(
            $"Couldn't add role {role} to account of id={userId}",
            result.Errors.Select(e => e.Description).ToList());
    }


    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto passData, Guid userId)
    {
        _logger.LogDebug("Changing password for user {UserId}", userId);

        Account? account = await _appUserManager.FindByIdAsync(userId.ToString());

        if (account == null)
        {
            _logger.LogError("Cannot change password; account {UserId} was not found", userId);
            return Result<bool>.NotFound($"Couldn't find account of id={userId}");
        }

        IdentityResult? result = await _appUserManager.ChangePasswordAsync(account, passData.CurrentPassword, passData.NewPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("Changed password for user {UserId}", userId);
            return Result<bool>.Success(true);
        }

        _logger.LogError("Failed to change password for user {UserId} due to {Errors}", userId, result.Errors);
        return Result<bool>.Validation(
            "Failed to change password. Please check the current password and try again.",
            result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<Result<UserResponseDto>> FindUserByEmailAsync(string email)
    {
        Account? account = await _appUserManager.FindByEmailAsync(email);
        if (account == null)
            return Result<UserResponseDto>.NotFound($"Couldn't find user with email {email}");
        
        return Result<UserResponseDto>.Success(_mapper.Map<UserResponseDto>(account));
    }

    public async Task<Result<bool>> IsAdminActivated(string email)
    {
        var account = await _appUserManager.Users.AsNoTracking()
                                                .OfType<Admin>()
                                                .FirstOrDefaultAsync(a => a.Email == email);

        if(account == null || account.IsActivated == false)                                            
            return Result<bool>.Success(false);
        
        return Result<bool>.Success(true);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginData)
    {
        _logger.LogDebug("Logging in account with email {Email}", loginData.Email);

        Account? account = await _appUserManager.FindByEmailAsync(loginData.Email);
        if (account == null)
        {
            _logger.LogDebug("No user found with email {Email}", loginData.Email);
            return Result<AuthResponseDto>.BadRequest("Invalid username or password");
        }


        SignInResult result = await _userSignInManager.PasswordSignInAsync(account, loginData.Password, false, false);

        if (result.Succeeded == false)
        {
            _logger.LogDebug("Invalid password for user with email {Email}", loginData.Email);
            return Result<AuthResponseDto>.BadRequest("Invalid username or password");
        }

        var roles = await _appUserManager.GetRolesAsync(account);
        var auth = _jwtTokenService.GenerateToken(account, roles);
        var refreshToken = GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(_jwtTokenService.JwtTokenOptions.RefreshTokenExpiryMinutes);
        await _appUserManager.UpdateAsync(account);

        _logger.LogInformation("Account {UserId} logged in with email {Email}", account.Id, loginData.Email);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            UserId: account.Id,
            AccessToken: auth.AccessToken,
            AccessTokenExpiry: auth.AccessTokenExpiry,
            RefreshToken: refreshToken,
            RefreshTokenExpiry: account.RefreshTokenExpiryDate
        ));
    }

    public async Task<Result<AuthResponseDto>> LoginWithGoogleAsync(string email)
    {       
        _logger.LogDebug("Logging in account with Google email {Email}", email);

        Account? account = await _appUserManager.FindByEmailAsync(email);
        if (account == null)
        {
            _logger.LogDebug("No user found with email {Email} for Google login", email);
            return Result<AuthResponseDto>.BadRequest("Google login failed");
        }

        var roles = await _appUserManager.GetRolesAsync(account);
        var auth = _jwtTokenService.GenerateToken(account, roles);
        var refreshToken = GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(_jwtTokenService.JwtTokenOptions.RefreshTokenExpiryMinutes);
        await _appUserManager.UpdateAsync(account);

        _logger.LogInformation("Account {UserId} logged in with Google email {Email}", account.Id, email);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            UserId: account.Id,
            AccessToken: auth.AccessToken,
            AccessTokenExpiry: auth.AccessTokenExpiry,
            RefreshToken: refreshToken,
            RefreshTokenExpiry: account.RefreshTokenExpiryDate
        ));
    }


    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(AuthResponseDto auth, Guid userId)
    {
        _logger.LogDebug("Refreshing token for user {UserId}", userId);

        // get user by id
        Account? account = await _appUserManager.FindByIdAsync(userId.ToString());

        if (account != null && account.RefreshToken == auth.RefreshToken && account.RefreshTokenExpiryDate > DateTime.UtcNow)
        {
            var roles = await _appUserManager.GetRolesAsync(account);
            var NewAuth = _jwtTokenService.GenerateToken(account, roles);

            var refreshToken = GenerateRefreshToken();
            account.RefreshToken = refreshToken;
            account.RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(_jwtTokenService.JwtTokenOptions.RefreshTokenExpiryMinutes);

            await _appUserManager.UpdateAsync(account);

            _logger.LogInformation("Refreshed token for user {UserId}", userId);
            return Result<AuthResponseDto>.Success(new AuthResponseDto(
                UserId: account.Id,
                AccessToken: NewAuth.AccessToken,
                AccessTokenExpiry: NewAuth.AccessTokenExpiry,
                RefreshToken: refreshToken,
                RefreshTokenExpiry: account.RefreshTokenExpiryDate
            ));
        }


        _logger.LogError("Invalid refresh token for user {UserId}", userId);
        return Result<AuthResponseDto>.BadRequest("Invalid refresh token");
    }

    public async Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto user, AppUserRole role = AppUserRole.USER)
    {
        _logger.LogDebug("Registering account with email {Email} as role {Role}", user.Email, role);

        if (await _appUserManager.FindByEmailAsync(user.Email) != null)
        {
            _logger.LogDebug("User with {Email} Already Exists", user.Email);
            return Result<RegisterResultDto>.Conflict($"User with {user.Email} Already Exists");
        }

        var result = role switch
        {
            AppUserRole.ADMIN => await _appUserManager.CreateAsync(_mapper.Map<Admin>(user), user.Password),
            AppUserRole.SUPER_ADMIN => await _appUserManager.CreateAsync(_mapper.Map<SuperAdmin>(user), user.Password),
            _ => await _appUserManager.CreateAsync(_mapper.Map<User>(user), user.Password)
        };

        Account? userResponse = null;
        if (result.Succeeded == false)
        {
            _logger.LogDebug("Couldn't register User with {Email} Due to {Errors}", user.Email, result.Errors);
            return Result<RegisterResultDto>.Validation(
                "Registration failed. Please check the data and try again.",
                result.Errors.Select(e => e.Description).ToList());
        }
        else
        {
            userResponse = await _appUserManager.FindByEmailAsync(user.Email);
            if (userResponse == null)
            {
                _logger.LogCritical("Registration for email {Email} succeeded but the account could not be loaded", user.Email);
            }

            _logger.LogInformation("Registered account with email {Email} as role {Role}", user.Email, role);
        }

        object? responseData = null;
        if (userResponse != null)
        {
            responseData = role switch
            {
                AppUserRole.ADMIN when userResponse is Admin admin => _mapper.Map<AdminResponseDto>(admin),
                AppUserRole.SUPER_ADMIN when userResponse is SuperAdmin superAdmin => _mapper.Map<SuperAdminResponseDto>(superAdmin),
                _ when userResponse is User appUser => _mapper.Map<UserResponseDto>(appUser),
                _ => null
            };
        }

        return Result<RegisterResultDto>.Success(new RegisterResultDto(
            Succeeded: result.Succeeded,
            Errors: result.Errors.Select(e => e.Description).ToList(),
            responseData: responseData
        ));
    }

    public async Task<Result<bool>> UpdateAccountAsync(UpdateAccountDto request, Guid userId)
    {
        _logger.LogDebug("Updating account {UserId}", userId);

        Account? account = await _appUserManager.FindByIdAsync(userId.ToString());
        if (account == null)
        {
            _logger.LogError("Cannot update account {UserId}; account was not found", userId);
            return Result<bool>.NotFound($"Couldn't find account of id={userId}");
        }

        account.FullName = request.FullName;
        account.PhoneNumber = request.PhoneNumber;

        var result = await _appUserManager.UpdateAsync(account);

        if (result.Succeeded)
        {
            _logger.LogInformation("Updated account {UserId}", userId);
            return Result<bool>.Success(true);
        }

        _logger.LogError("Failed to update account {UserId} due to {Errors}", userId, result.Errors);
        return Result<bool>.Validation(
            "Failed to update account. Please check the data and try again.",
            result.Errors.Select(e => e.Description).ToList());
    }


    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
