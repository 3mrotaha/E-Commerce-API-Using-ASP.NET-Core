using System.Net;
using System.Text.Json;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// Cross-cutting tests for behaviour that lives in the pipeline rather than any single controller:
/// the global authentication requirement, content-type negotiation, unknown routes, and the shape of
/// error responses produced by the exception middleware.
/// </summary>
public class AuthAndPipelineTests : IntegrationTestBase
{
    public AuthAndPipelineTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutIdentity_ShouldReturnUnauthorized()
    {
        // Every endpoint is protected by default via the global AuthorizeFilter.
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnknownRoute_ShouldReturnNotFound()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/this-route-does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Request_WithUnsupportedContentType_ShouldReturnUnsupportedMediaType()
    {
        // The global [Consumes("application/json")] filter rejects non-JSON request bodies with 415.
        var client = CreateAnonymousClient();
        var content = RawContent("just plain text", "text/plain");

        var response = await client.PostAsync("/api/Account/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task ErrorResponse_ShouldHaveMessageProperty()
    {
        // A NotFound result flows through the controller base / exception handling and returns a JSON
        // body shaped like { "message": "..." }. Verify that contract holds end to end.
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Products/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        json.RootElement.TryGetProperty("message", out _).Should().BeTrue();
    }
}
