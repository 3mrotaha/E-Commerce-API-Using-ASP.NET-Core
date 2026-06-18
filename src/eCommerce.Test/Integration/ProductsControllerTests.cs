using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.Product;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Products. These drive the real HTTP pipeline: routing, the global
/// "must be authenticated" filter, role checks for write operations, and the service/EF round trip.
/// </summary>
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProducts_WhenAuthenticated_ShouldReturnSeededProducts()
    {
        // Arrange: any signed-in role may browse products.
        var client = CreateUserClient(Guid.NewGuid());

        // Act: page 1, up to 50 items so the six seeded products fit on one page.
        var response = await client.GetAsync("/api/Products/1/50");

        // Assert: 200 and the seeded catalogue comes back.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await ReadAsAsync<List<ProductResponseDto>>(response);
        products.Should().Contain(p => p.Id == SeedIds.NoiseCancellingHeadphonesId);
    }

    [Fact]
    public async Task GetProducts_WhenAnonymous_ShouldReturnUnauthorized()
    {
        // Arrange: no identity headers means the request is unauthenticated.
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync("/api/Products/1/50");

        // Assert: the global AuthorizeFilter rejects anonymous callers with 401.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProduct_WhenExists_ShouldReturnProduct()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Products/{SeedIds.MechanicalKeyboardId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await ReadAsAsync<ProductResponseDto>(response);
        product.Id.Should().Be(SeedIds.MechanicalKeyboardId);
    }

    [Fact]
    public async Task GetProduct_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Products/{SeedIds.NonExistentId}");

        // The service returns a NotFound Result, which the controller base maps to 404.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductsByCategory_WhenCategoryMissing_ShouldReturnNotFound()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Products/category/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddProduct_AsAdmin_ShouldCreateProduct()
    {
        // Arrange: writing to the catalogue requires ADMIN or SUPER_ADMIN.
        var client = CreateSuperAdminClient();
        var newProduct = new CreateProductDto(
            CategoryId: SeedIds.ElectronicsCategoryId,
            Name: "Test Webcam",
            Description: "A product created by an integration test.",
            QuantityInStock: 10,
            UnitPrice: 59.99m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Products", newProduct);

        // Assert: the controller returns the created product via Ok (200), echoing the new id.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await ReadAsAsync<ProductResponseDto>(response);
        created.Name.Should().Be("Test Webcam");
        created.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddProduct_AsUser_ShouldReturnForbidden()
    {
        // Arrange: a plain USER lacks the role required to create products.
        var client = CreateUserClient(Guid.NewGuid());
        var newProduct = new CreateProductDto(SeedIds.ElectronicsCategoryId, "Nope", null, 1, 1m);

        // Act
        var response = await client.PostAsJsonAsync("/api/Products", newProduct);

        // Assert: authenticated but unauthorized => 403 (not 401).
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddProduct_WhenCategoryMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();
        var newProduct = new CreateProductDto(SeedIds.NonExistentId, "Orphan", null, 1, 1m);

        var response = await client.PostAsJsonAsync("/api/Products", newProduct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_AsAdmin_ShouldReturnUpdatedProduct()
    {
        var client = CreateSuperAdminClient();
        var update = new UpdateProductDto(SeedIds.FitnessCategoryId, "Balance Grip Yoga Mat v2", "Updated", 80, 44.95m);

        var response = await client.PutAsJsonAsync($"/api/Products/{SeedIds.YogaMatId}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<ProductResponseDto>(response);
        updated.Name.Should().Be("Balance Grip Yoga Mat v2");
    }

    [Fact]
    public async Task UpdateProduct_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();
        var update = new UpdateProductDto(null, "Ghost", null, 1, 1m);

        var response = await client.PutAsJsonAsync($"/api/Products/{SeedIds.NonExistentId}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_AsAdmin_ShouldSucceed()
    {
        // Arrange: create a throwaway product first so the delete doesn't disturb shared seed data.
        var client = CreateSuperAdminClient();
        var create = await client.PostAsJsonAsync("/api/Products",
            new CreateProductDto(SeedIds.ElectronicsCategoryId, "Disposable", null, 5, 9.99m));
        var created = await ReadAsAsync<ProductResponseDto>(create);

        // Act
        var response = await client.DeleteAsync($"/api/Products/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProduct_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();

        var response = await client.DeleteAsync($"/api/Products/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
