using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Account. These cover the public auth flows (register, login, refresh) and
/// the protected, role- and ownership-gated account operations. Register and login are normally rate
/// limited; the test host relaxes that so the flows can run freely.
/// </summary>
public class AccountControllerTests : IntegrationTestBase
{
    public AccountControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    // A password that satisfies the configured Identity policy (>=8 chars, upper, lower, digit).
    private const string ValidPassword = "Password1";

    private static RegisterUserDto NewUser(string? email = null)
    {
        var unique = Guid.NewGuid().ToString("N");
        return new RegisterUserDto(
            FullName: "Test User",
            UserName: $"user_{unique}",
            Email: email ?? $"user_{unique}@test.com",
            Password: ValidPassword,
            PhoneNumber: null);
    }

    /// <summary>Registers a user and returns the new account id (read from the response payload).</summary>
    private async Task<(Guid Id, RegisterUserDto Dto)> RegisterUserAsync(HttpClient client)
    {
        var dto = NewUser();
        var response = await client.PostAsJsonAsync("/api/Account/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await ReadAsAsync<RegisterResultDto>(response);
        result.Succeeded.Should().BeTrue();

        // responseData is serialised as a JSON object; pull the new account id out of it.
        var id = ((JsonElement)result.responseData!).GetProperty("id").GetGuid();
        return (id, dto);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldSucceed()
    {
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync("/api/Account/register", NewUser());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadAsAsync<RegisterResultDto>(response);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var client = CreateAnonymousClient();
        var dto = NewUser();
        await client.PostAsJsonAsync("/api/Account/register", dto);

        // Registering the same email again conflicts; Conflict maps to 400 at the HTTP layer.
        var response = await client.PostAsJsonAsync("/api/Account/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingFields_ShouldReturnBadRequest()
    {
        var client = CreateAnonymousClient();

        // An empty body leaves required (non-nullable) fields null; [ApiController] auto-validation rejects it.
        var response = await client.PostAsJsonAsync("/api/Account/register", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var client = CreateAnonymousClient();
        var (_, dto) = await RegisterUserAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/login", new LoginDto(dto.Email, dto.Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await ReadAsAsync<AuthResponseDto>(response);
        auth.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        var client = CreateAnonymousClient();
        var (_, dto) = await RegisterUserAsync(client);

        var response = await client.PostAsJsonAsync("/api/Account/login", new LoginDto(dto.Email, "WrongPass1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewToken()
    {
        // Arrange: register + login to obtain a real refresh token.
        var client = CreateAnonymousClient();
        var (_, dto) = await RegisterUserAsync(client);
        var login = await ReadAsAsync<AuthResponseDto>(
            await client.PostAsJsonAsync("/api/Account/login", new LoginDto(dto.Email, dto.Password)));

        // Act: exchange the refresh token for a new access token.
        var response = await client.PostAsJsonAsync($"/api/Account/refresh-token/{login.UserId}", login);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await ReadAsAsync<AuthResponseDto>(response);
        refreshed.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var client = CreateAnonymousClient();
        var (id, _) = await RegisterUserAsync(client);
        var bogus = new AuthResponseDto("access", DateTime.UtcNow.AddMinutes(5), id, "not-a-real-refresh-token", DateTime.UtcNow.AddDays(1));

        var response = await client.PostAsJsonAsync($"/api/Account/refresh-token/{id}", bogus);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAdmin_AsSuperAdmin_ShouldSucceed()
    {
        var client = CreateSuperAdminClient();

        var response = await client.PostAsJsonAsync("/api/Account/register-admin", NewUser());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterAdmin_AsUser_ShouldReturnForbidden()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/Account/register-admin", NewUser());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterSuperAdmin_AsSuperAdmin_ShouldSucceed()
    {
        var client = CreateSuperAdminClient();

        var response = await client.PostAsJsonAsync("/api/Account/register-superadmin", NewUser());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateAdmin_AsSuperAdmin_ShouldSucceed()
    {
        // Arrange: create an admin, then read its id back from the registration payload.
        var client = CreateSuperAdminClient();
        var register = await client.PostAsJsonAsync("/api/Account/register-admin", NewUser());
        var result = await ReadAsAsync<RegisterResultDto>(register);
        var adminId = ((JsonElement)result.responseData!).GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsync($"/api/Account/activate/admin/{adminId}", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateAdmin_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();

        var response = await client.PutAsync($"/api/Account/activate/admin/{SeedIds.NonExistentId}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAccount_AsOwner_ShouldSucceed()
    {
        // Arrange: register a user, then act as that user to update their own account.
        var anonymous = CreateAnonymousClient();
        var (id, dto) = await RegisterUserAsync(anonymous);
        var client = CreateClientAs(id, dto.Email, UserRole);

        // Act
        var response = await client.PutAsJsonAsync($"/api/Account/update-account/{id}",
            new UpdateAccountDto("Updated Name", "+1000000000"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateAccount_ForAnotherUser_ShouldReturnUnauthorized()
    {
        // Arrange: signed in as one user, targeting another user's account without privilege.
        var caller = Guid.NewGuid();
        var client = CreateUserClient(caller);

        var response = await client.PutAsJsonAsync($"/api/Account/update-account/{Guid.NewGuid()}",
            new UpdateAccountDto("Hacker", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_AsOwner_ShouldSucceed()
    {
        var anonymous = CreateAnonymousClient();
        var (id, dto) = await RegisterUserAsync(anonymous);
        var client = CreateClientAs(id, dto.Email, UserRole);

        var response = await client.PostAsJsonAsync($"/api/Account/change-password/{id}",
            new ChangePasswordDto(ValidPassword, "NewPassword2"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_ShouldSucceed()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/Account/logout");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
