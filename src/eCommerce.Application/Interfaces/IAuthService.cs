using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Common;
using eCommerce.Domain.Enums;

namespace eCommerce.Application.Interfaces;

public interface IAuthService
{        
    // register
    Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto user, AppUserRole role = AppUserRole.USER);
    // create role
    Task<Result<bool>> AddUserRole(Guid UserId, AppUserRole Role);

    // find user by email
    Task<Result<UserResponseDto>> FindUserByEmailAsync(string email);
    
    // login
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginData);
    Task<Result<AuthResponseDto>> LoginWithGoogleAsync(string email);
    // refresh token
    Task<Result<AuthResponseDto>> RefreshTokenAsync(AuthResponseDto auth, Guid userId);

    // update account details
    Task<Result<bool>> UpdateAccountAsync(UpdateAccountDto request,  Guid userId);

    // Change Password
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto passData,  Guid userId);
    Task<Result<bool>> IsAdminActivated(string email);
    Task<Result<bool>> ActivateAdminAccount(Guid adminId);
}
