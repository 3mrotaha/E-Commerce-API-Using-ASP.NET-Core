using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class ProductReviewRepository_Test
{
    private static ProductReviewRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<ProductReviewRepository>.Instance);

    private static ProductReview NewReview(Guid? productId = null, int stars = 5)
        => new() { Id = Guid.NewGuid(), ProductId = productId ?? Guid.NewGuid(), UserId = Guid.NewGuid(), Stars = stars };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var review = NewReview();

        await repo.AddAsync(review);

        (await repo.GetByIdAsync(review.Id))!.Stars.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByProduct_ShouldReturnMatchingReviews()
    {
        var repo = NewRepo();
        var productId = Guid.NewGuid();
        await repo.AddAsync(NewReview(productId));
        await repo.AddAsync(NewReview(Guid.NewGuid()));

        var found = await repo.FindAsync(r => r.ProductId == productId);

        found.Should().ContainSingle().Which.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var review = await repo.AddAsync(NewReview());
        review!.Comment = "edited";

        await repo.UpdateAsync(review);

        (await repo.GetByIdAsync(review.Id))!.Comment.Should().Be("edited");
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var review = await repo.AddAsync(NewReview());

        await repo.DeleteAsync(review!);

        (await repo.GetByIdAsync(review!.Id)).Should().BeNull();
    }
}
