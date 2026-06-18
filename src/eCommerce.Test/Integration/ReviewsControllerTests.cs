using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/Reviews. Writing a review is restricted to the USER role and to the user
/// named in the route (ownership), while reads are open to any signed-in account.
/// </summary>
public class ReviewsControllerTests : IntegrationTestBase
{
    public ReviewsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>Creates a review for a fresh user/product pair and returns it.</summary>
    private async Task<ProductReviewResponseDto> CreateReviewAsync(HttpClient client, Guid userId, Guid productId)
    {
        var response = await client.PostAsJsonAsync($"/api/Reviews/{userId}",
            new CreateProductReviewDto(productId, 5, "Great product."));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadAsAsync<ProductReviewResponseDto>(response);
    }

    [Fact]
    public async Task AddReview_AsOwningUser_ShouldCreateReview()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var review = await CreateReviewAsync(client, userId, SeedIds.SmartWatchId);

        review.UserId.Should().Be(userId);
        review.ProductId.Should().Be(SeedIds.SmartWatchId);
        review.Stars.Should().Be(5);
    }

    [Fact]
    public async Task AddReview_ForAnotherUser_ShouldReturnUnauthorized()
    {
        // Arrange: signed in as one user, but posting under another user's id in the route.
        var caller = Guid.NewGuid();
        var other = Guid.NewGuid();
        var client = CreateUserClient(caller);

        var response = await client.PostAsJsonAsync($"/api/Reviews/{other}",
            new CreateProductReviewDto(SeedIds.SmartWatchId, 5, "x"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddReview_AsSuperAdmin_ShouldReturnForbidden()
    {
        // Arrange: only the USER role may write reviews; an admin is forbidden.
        var userId = Guid.NewGuid();
        var client = CreateSuperAdminClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Reviews/{userId}",
            new CreateProductReviewDto(SeedIds.SmartWatchId, 5, "x"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddReview_WithInvalidStars_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/Reviews/{userId}",
            new CreateProductReviewDto(SeedIds.SmartWatchId, 0, "x"));

        // Stars must be 1..5; the service returns a Validation result => 400.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReview_WhenExists_ShouldReturnReview()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await CreateReviewAsync(client, userId, SeedIds.ErgonomicChairId);

        var response = await client.GetAsync($"/api/Reviews/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var review = await ReadAsAsync<ProductReviewResponseDto>(response);
        review.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetReview_WhenMissing_ShouldReturnNotFound()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Reviews/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReviewsByProduct_ShouldReturnSeededReviews()
    {
        var client = CreateUserClient(Guid.NewGuid());

        // The seeded headphones product has reviews from the seeded users.
        var response = await client.GetAsync($"/api/Reviews/product/{SeedIds.ReviewedProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviews = await ReadAsAsync<List<ProductReviewResponseDto>>(response);
        reviews.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetReviewsByProduct_WhenProductMissing_ShouldReturnNotFound()
    {
        var client = CreateUserClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/Reviews/product/{SeedIds.NonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReview_AsOwningUser_ShouldReturnUpdatedReview()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await CreateReviewAsync(client, userId, SeedIds.StandingDeskId);

        var response = await client.PutAsJsonAsync($"/api/Reviews/{userId}/{created.Id}",
            new UpdateProductReviewDto(3, "Changed my mind."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<ProductReviewResponseDto>(response);
        updated.Stars.Should().Be(3);
    }

    [Fact]
    public async Task DeleteReview_AsOwningUser_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await CreateReviewAsync(client, userId, SeedIds.MechanicalKeyboardId);

        var response = await client.DeleteAsync($"/api/Reviews/{userId}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddReview_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync($"/api/Reviews/{userId}",
            new CreateProductReviewDto(SeedIds.SmartWatchId, 5, "x"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
