using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Application.Interfaces;
using eCommerce.Application.Services;
using eCommerce.Domain.Enums;
using eCommerce.Domain.Identity;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class AuthService_Test
{
    private readonly Mock<UserManager<Account>> _userManager = IdentityMockFactory.CreateUserManager();
    private readonly Mock<RoleManager<ApplicationRole>> _roleManager = IdentityMockFactory.CreateRoleManager();
    private readonly Mock<SignInManager<Account>> _signInManager;
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly AuthService _sut;

    public AuthService_Test()
    {
        _signInManager = IdentityMockFactory.CreateSignInManager(_userManager.Object);
        _jwt.SetupGet(j => j.JwtTokenOptions).Returns(new JwtTokenOptions { RefreshTokenExpiryMinutes = 60 });
        _jwt.Setup(j => j.GenerateToken(It.IsAny<Account>(), It.IsAny<IEnumerable<string>>()))
            .Returns(new AuthResponseDto("access-token", DateTime.UtcNow.AddMinutes(10)));
        _sut = new AuthService(_userManager.Object, _roleManager.Object, _signInManager.Object,
            _mapper.Object, NullLogger<AuthService>.Instance, _jwt.Object);
    }

    // Backs UserManager.Users with an async-capable queryable so EF's FirstOrDefaultAsync works.
    private void SetupUsers(params Account[] accounts) =>
        _userManager.Setup(m => m.Users).Returns(accounts.AsAsyncQueryable());

    private static IdentityResult Failed() => IdentityResult.Failed(new IdentityError { Description = "boom" });

    // ---- ActivateAdminAccount ----

    [Fact]
    public async Task ActivateAdminAccount_WhenAdminNotFound_ShouldReturnNotFound()
    {
        SetupUsers(); // no admins

        var result = await _sut.ActivateAdminAccount(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ActivateAdminAccount_WhenUpdateFails_ShouldReturnBadRequest()
    {
        var admin = new Admin { Id = Guid.NewGuid() };
        SetupUsers(admin);
        _userManager.Setup(m => m.UpdateAsync(It.IsAny<Account>())).ReturnsAsync(Failed());

        var result = await _sut.ActivateAdminAccount(admin.Id);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task ActivateAdminAccount_WhenUpdateSucceeds_ShouldActivateAndReturnSuccess()
    {
        var admin = new Admin { Id = Guid.NewGuid(), IsActivated = false };
        SetupUsers(admin);
        _userManager.Setup(m => m.UpdateAsync(It.IsAny<Account>())).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ActivateAdminAccount(admin.Id);

        result.IsSuccess.Should().BeTrue();
        admin.IsActivated.Should().BeTrue();
    }

    // ---- AddUserRole ----

    [Fact]
    public async Task AddUserRole_WhenRoleCreationFails_ShouldReturnValidation()
    {
        _roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _roleManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>())).ReturnsAsync(Failed());

        var result = await _sut.AddUserRole(Guid.NewGuid(), AppUserRole.ADMIN);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddUserRole_WhenAccountNotFound_ShouldReturnNotFound()
    {
        _roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.AddUserRole(Guid.NewGuid(), AppUserRole.ADMIN);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AddUserRole_WhenAddToRoleSucceeds_ShouldReturnSuccess()
    {
        _roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new Admin());
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<Account>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.AddUserRole(Guid.NewGuid(), AppUserRole.ADMIN);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddUserRole_WhenAddToRoleFails_ShouldReturnValidation()
    {
        _roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<Account>(), It.IsAny<string>())).ReturnsAsync(Failed());

        var result = await _sut.AddUserRole(Guid.NewGuid(), AppUserRole.USER);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    // ---- ChangePasswordAsync ----

    [Fact]
    public async Task ChangePasswordAsync_WhenAccountNotFound_ShouldReturnNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.ChangePasswordAsync(new ChangePasswordDto("old", "new"), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenIdentityFails_ShouldReturnValidation()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _userManager.Setup(m => m.ChangePasswordAsync(It.IsAny<Account>(), "old", "new")).ReturnsAsync(Failed());

        var result = await _sut.ChangePasswordAsync(new ChangePasswordDto("old", "new"), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSucceeds_ShouldReturnSuccess()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _userManager.Setup(m => m.ChangePasswordAsync(It.IsAny<Account>(), "old", "new")).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ChangePasswordAsync(new ChangePasswordDto("old", "new"), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
    }

    // ---- FindUserByEmailAsync ----

    [Fact]
    public async Task FindUserByEmailAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.FindUserByEmailAsync("missing@example.com");

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task FindUserByEmailAsync_WhenFound_ShouldReturnMappedUser()
    {
        var account = new User { Id = Guid.NewGuid(), Email = "u@e.com" };
        _userManager.Setup(m => m.FindByEmailAsync(account.Email)).ReturnsAsync(account);
        _mapper.Setup(m => m.Map<UserResponseDto>(account))
               .Returns(new UserResponseDto(account.Id, "U", "u", account.Email!, null, false, 0, default));

        var result = await _sut.FindUserByEmailAsync(account.Email!);

        result.IsSuccess.Should().BeTrue();
    }

    // ---- IsAdminActivated ----

    [Fact]
    public async Task IsAdminActivated_WhenNoAdmin_ShouldReturnFalse()
    {
        SetupUsers();

        var result = await _sut.IsAdminActivated("admin@example.com");

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsAdminActivated_WhenAdminNotActivated_ShouldReturnFalse()
    {
        SetupUsers(new Admin { Email = "admin@example.com", IsActivated = false });

        var result = await _sut.IsAdminActivated("admin@example.com");

        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsAdminActivated_WhenAdminActivated_ShouldReturnTrue()
    {
        SetupUsers(new Admin { Email = "admin@example.com", IsActivated = true });

        var result = await _sut.IsAdminActivated("admin@example.com");

        result.Value.Should().BeTrue();
    }

    // ---- LoginAsync ----

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnBadRequest()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.LoginAsync(new LoginDto("u@e.com", "pass"));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ShouldReturnBadRequest()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _signInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<Account>(), It.IsAny<string>(), false, false))
                      .ReturnsAsync(SignInResult.Failed);

        var result = await _sut.LoginAsync(new LoginDto("u@e.com", "wrong"));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ShouldReturnTokenAndRefreshToken()
    {
        var account = new User { Id = Guid.NewGuid(), Email = "u@e.com" };
        _userManager.Setup(m => m.FindByEmailAsync(account.Email)).ReturnsAsync(account);
        _signInManager.Setup(m => m.PasswordSignInAsync(account, "pass", false, false)).ReturnsAsync(SignInResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(account)).ReturnsAsync(new List<string> { "USER" });
        _userManager.Setup(m => m.UpdateAsync(account)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginAsync(new LoginDto(account.Email!, "pass"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        account.RefreshToken.Should().NotBeNullOrEmpty();
    }

    // ---- LoginWithGoogleAsync ----

    [Fact]
    public async Task LoginWithGoogleAsync_WhenUserNotFound_ShouldReturnBadRequest()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.LoginWithGoogleAsync("u@e.com");

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_WhenUserExists_ShouldReturnToken()
    {
        var account = new User { Id = Guid.NewGuid(), Email = "u@e.com" };
        _userManager.Setup(m => m.FindByEmailAsync(account.Email)).ReturnsAsync(account);
        _userManager.Setup(m => m.GetRolesAsync(account)).ReturnsAsync(new List<string>());
        _userManager.Setup(m => m.UpdateAsync(account)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginWithGoogleAsync(account.Email!);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
    }

    // ---- RefreshTokenAsync ----

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenInvalid_ShouldReturnBadRequest()
    {
        var account = new User { Id = Guid.NewGuid(), RefreshToken = "stored", RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(account);

        var result = await _sut.RefreshTokenAsync(new AuthResponseDto("a", default, RefreshToken: "different"), account.Id);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenExpired_ShouldReturnBadRequest()
    {
        var account = new User { Id = Guid.NewGuid(), RefreshToken = "stored", RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(-5) };
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(account);

        var result = await _sut.RefreshTokenAsync(new AuthResponseDto("a", default, RefreshToken: "stored"), account.Id);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenValid_ShouldIssueNewTokens()
    {
        var account = new User { Id = Guid.NewGuid(), RefreshToken = "stored", RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(30) };
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(account);
        _userManager.Setup(m => m.GetRolesAsync(account)).ReturnsAsync(new List<string>());
        _userManager.Setup(m => m.UpdateAsync(account)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RefreshTokenAsync(new AuthResponseDto("a", default, RefreshToken: "stored"), account.Id);

        result.IsSuccess.Should().BeTrue();
        account.RefreshToken.Should().NotBe("stored"); // rotated
    }

    // ---- RegisterUserAsync ----

    [Fact]
    public async Task RegisterUserAsync_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User());

        var result = await _sut.RegisterUserAsync(NewRegisterDto());

        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenCreateFails_ShouldReturnValidation()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);
        _mapper.Setup(m => m.Map<User>(It.IsAny<RegisterUserDto>())).Returns(new User());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<Account>(), It.IsAny<string>())).ReturnsAsync(Failed());

        var result = await _sut.RegisterUserAsync(NewRegisterDto());

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenUserCreated_ShouldReturnSuccess()
    {
        var created = new User { Id = Guid.NewGuid(), Email = "new@e.com" };
        // First lookup (existence check) → null; second lookup (load created) → the user.
        _userManager.SetupSequence(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((Account?)null)
                    .ReturnsAsync(created);
        _mapper.Setup(m => m.Map<User>(It.IsAny<RegisterUserDto>())).Returns(created);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<Account>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mapper.Setup(m => m.Map<UserResponseDto>(created))
               .Returns(new UserResponseDto(created.Id, "N", "n", created.Email!, null, false, 0, default));

        var result = await _sut.RegisterUserAsync(NewRegisterDto(), AppUserRole.USER);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterUserAsync_WhenAdminCreated_ShouldReturnSuccess()
    {
        var created = new Admin { Id = Guid.NewGuid(), Email = "admin@e.com" };
        _userManager.SetupSequence(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((Account?)null)
                    .ReturnsAsync(created);
        _mapper.Setup(m => m.Map<Admin>(It.IsAny<RegisterUserDto>())).Returns(created);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<Account>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _mapper.Setup(m => m.Map<AdminResponseDto>(created)).Returns(new AdminResponseDto(created.Id, "A", "a", created.Email!, null, false, default));

        var result = await _sut.RegisterUserAsync(NewRegisterDto(), AppUserRole.ADMIN);

        result.IsSuccess.Should().BeTrue();
    }

    // ---- UpdateAccountAsync ----

    [Fact]
    public async Task UpdateAccountAsync_WhenAccountNotFound_ShouldReturnNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        var result = await _sut.UpdateAccountAsync(new UpdateAccountDto("Name", null), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenUpdateFails_ShouldReturnValidation()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        _userManager.Setup(m => m.UpdateAsync(It.IsAny<Account>())).ReturnsAsync(Failed());

        var result = await _sut.UpdateAccountAsync(new UpdateAccountDto("Name", null), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenValid_ShouldCopyFieldsAndReturnSuccess()
    {
        var account = new User();
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(account);
        _userManager.Setup(m => m.UpdateAsync(account)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateAccountAsync(new UpdateAccountDto("Jane", "12345"), Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        account.FullName.Should().Be("Jane");
        account.PhoneNumber.Should().Be("12345");
    }

    private static RegisterUserDto NewRegisterDto() => new("Full Name", "username", "new@e.com", "P@ssw0rd!", null);
}
