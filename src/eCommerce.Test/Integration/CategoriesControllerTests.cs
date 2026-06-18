using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Categories. Reads are open to any signed-in user; writes are admin-only.
/// </summary>
public class CategoriesControllerTests : IntegrationTestBase
{
    public CategoriesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAllCategories_AsUser_ShouldReturnSeededCategories()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/Categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await ReadAsAsync<List<ProductCategoryResponseDto>>(response);
        categories.Should().Contain(c => c.Id == SeedIds.ElectronicsCategoryId);
    }

    [Fact]
    public async Task GetAllCategories_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/Categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddCategory_AsAdmin_ShouldCreateCategory()
    {
        var client = CreateSuperAdminClient();
        var newCategory = new CreateProductCategoryDto("Gaming", "Gaming gear and accessories.");

        var response = await client.PostAsJsonAsync("/api/Categories", newCategory);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await ReadAsAsync<ProductCategoryResponseDto>(response);
        created.Name.Should().Be("Gaming");
        created.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddCategory_AsUser_ShouldReturnForbidden()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/Categories", new CreateProductCategoryDto("Nope", null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_AsAdmin_ShouldReturnUpdatedCategory()
    {
        // Arrange: create one so the update is independent of shared seed rows.
        var client = CreateSuperAdminClient();
        var create = await client.PostAsJsonAsync("/api/Categories", new CreateProductCategoryDto("Temp", "temp"));
        var created = await ReadAsAsync<ProductCategoryResponseDto>(create);

        // Act
        var response = await client.PutAsJsonAsync($"/api/Categories/{created.Id}",
            new UpdateProductCategoryDto("Temp Updated", "updated"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<ProductCategoryResponseDto>(response);
        updated.Name.Should().Be("Temp Updated");
    }

    [Fact]
    public async Task UpdateCategory_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();

        var response = await client.PutAsJsonAsync($"/api/Categories/{SeedIds.NonExistentId}",
            new UpdateProductCategoryDto("Ghost", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_AsAdmin_ShouldSucceed()
    {
        var client = CreateSuperAdminClient();
        var create = await client.PostAsJsonAsync("/api/Categories", new CreateProductCategoryDto("ToDelete", null));
        var created = await ReadAsAsync<ProductCategoryResponseDto>(create);

        var response = await client.DeleteAsync($"/api/Categories/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCategory_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateSuperAdminClient();

        var response = await client.DeleteAsync($"/api/Categories/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
