using eCommerce.Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace eCommerce.Test.Common;

/// <summary>
/// Factory for the ASP.NET Core Identity managers used by AuthService. These are concrete classes
/// (not interfaces), but their members are virtual, so Moq can mock them once we satisfy their
/// constructor dependencies with mocked stores / null collaborators.
/// </summary>
public static class IdentityMockFactory
{
    public static Mock<UserManager<Account>> CreateUserManager()
    {
        var store = new Mock<IUserStore<Account>>();
        // Remaining ctor args (options, hashers, validators, logger...) are unused by our tests → null.
        return new Mock<UserManager<Account>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static Mock<RoleManager<ApplicationRole>> CreateRoleManager()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store.Object, null!, null!, null!, null!);
    }

    public static Mock<SignInManager<Account>> CreateSignInManager(UserManager<Account> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Account>>();
        return new Mock<SignInManager<Account>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            null!,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<Account>>().Object);
    }
}
