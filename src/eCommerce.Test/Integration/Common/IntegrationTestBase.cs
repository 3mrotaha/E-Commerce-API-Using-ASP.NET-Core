using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace eCommerce.Test.Integration.Common;

/// <summary>
/// Shared base for the HTTP-level integration tests. It owns the in-process server (via the shared
/// <see cref="CustomWebApplicationFactory"/>) and exposes small helpers that keep the individual tests
/// short: building clients with a given identity, and reading/writing JSON.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    // Roles as the API spells them (matching the AppUserRole enum and [Authorize(Roles = ...)] strings).
    protected const string UserRole = "USER";
    protected const string AdminRole = "ADMIN";
    protected const string SuperAdminRole = "SUPER_ADMIN";

    // The API serialises responses with camelCase; mirror that when deserialising in tests.
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        // Make sure the seed rows exist before the first request runs.
        Factory.EnsureDatabaseSeeded();
    }

    /// <summary>A client with no identity headers — every protected endpoint should reject it with 401.</summary>
    protected HttpClient CreateAnonymousClient()
    {
        var client = Factory.CreateClient();
        // The API uses header-based versioning with a default of 1.0; set it explicitly for clarity.
        client.DefaultRequestHeaders.Add("x-api-version", "1.0");
        return client;
    }

    /// <summary>A client that authenticates as the given user id with the given roles.</summary>
    protected HttpClient CreateClientAs(Guid userId, string email, params string[] roles)
    {
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        if (roles.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(',', roles));
        }

        return client;
    }

    /// <summary>A regular USER. A fresh id per call keeps user-scoped state (carts, orders) isolated.</summary>
    protected HttpClient CreateUserClient(Guid userId) =>
        CreateClientAs(userId, $"user-{userId:N}@test.com", UserRole);

    /// <summary>A SUPER_ADMIN, used to exercise admin-only endpoints without tripping the admin-activation middleware.</summary>
    protected HttpClient CreateSuperAdminClient(Guid? userId = null)
    {
        var id = userId ?? Guid.NewGuid();
        return CreateClientAs(id, $"superadmin-{id:N}@test.com", SuperAdminRole);
    }

    /// <summary>Deserialises a successful response body into <typeparamref name="T"/>.</summary>
    protected static async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(content, JsonOptions);
        Assert.NotNull(value);
        return value!;
    }

    /// <summary>Wraps a raw string body with a chosen content type — handy for negative content-type tests.</summary>
    protected static StringContent RawContent(string body, string contentType) =>
        new(body, Encoding.UTF8, new MediaTypeHeaderValue(contentType));
}
