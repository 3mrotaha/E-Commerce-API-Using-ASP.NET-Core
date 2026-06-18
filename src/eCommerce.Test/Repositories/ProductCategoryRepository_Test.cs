using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class ProductCategoryRepository_Test
{
    private static ProductCategoryRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<ProductCategoryRepository>.Instance);

    private static ProductCategory NewCategory(string name = "Test Category")
        => new() { Id = Guid.NewGuid(), Name = name };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var category = NewCategory();

        await repo.AddAsync(category);

        (await repo.GetByIdAsync(category.Id))!.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldIncludeSeededAndAddedCategories()
    {
        var repo = NewRepo();
        var category = await repo.AddAsync(NewCategory("Brand New Category"));

        var all = await repo.GetAllAsync();

        all.Should().Contain(c => c.Id == category!.Id);
        all.Count().Should().BeGreaterThan(3); // 3 seeded + the one added
    }

    [Fact]
    public async Task FindAsync_WhenPredicateMatches_ShouldReturnOnlyMatching()
    {
        var repo = NewRepo();
        var category = await repo.AddAsync(NewCategory("UniqueCategoryName"));

        var found = await repo.FindAsync(c => c.Name == "UniqueCategoryName");

        found.Should().ContainSingle().Which.Id.Should().Be(category!.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var category = await repo.AddAsync(NewCategory());
        category!.Description = "updated";

        await repo.UpdateAsync(category);

        (await repo.GetByIdAsync(category.Id))!.Description.Should().Be("updated");
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var category = await repo.AddAsync(NewCategory());

        await repo.DeleteAsync(category!);

        (await repo.GetByIdAsync(category!.Id)).Should().BeNull();
    }
}
