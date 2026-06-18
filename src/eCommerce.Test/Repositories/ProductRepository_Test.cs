using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

/// <summary>
/// Products are part of the HasData seed (6 rows), so assertions use "contains"/unique predicates
/// rather than absolute counts of the whole table.
/// </summary>
public class ProductRepository_Test
{
    private static ProductRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<ProductRepository>.Instance);

    private static Product NewProduct(string name = "Test Product", decimal price = 10m)
        => new() { Id = Guid.NewGuid(), Name = name, UnitPrice = price, QuantityInStock = 5, IsDeleted = false };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var product = NewProduct();

        await repo.AddAsync(product);

        (await repo.GetByIdAsync(product.Id))!.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_WhenPredicateMatches_ShouldReturnOnlyMatching()
    {
        var repo = NewRepo();
        var product = NewProduct("UniquePredicateProduct");
        await repo.AddAsync(product);

        var found = await repo.FindAsync(p => p.Name == "UniquePredicateProduct");

        found.Should().ContainSingle().Which.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var product = await repo.AddAsync(NewProduct());
        product!.UnitPrice = 99.99m;

        await repo.UpdateAsync(product);

        (await repo.GetByIdAsync(product.Id))!.UnitPrice.Should().Be(99.99m);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var product = await repo.AddAsync(NewProduct());

        await repo.DeleteAsync(product!);

        (await repo.GetByIdAsync(product!.Id)).Should().BeNull();
    }

    [Fact]
    public async Task PagedFindAsync_WhenMoreResultsThanPageSize_ShouldReturnRequestedPage()
    {
        var repo = NewRepo();
        for (var i = 0; i < 3; i++)
        {
            await repo.AddAsync(NewProduct($"PAGED Product {i}", price: 5m));
        }

        var page1 = await repo.FindAsync(p => p.Name.StartsWith("PAGED"), pageNumber: 1, pageSize: 2);
        var page2 = await repo.FindAsync(p => p.Name.StartsWith("PAGED"), pageNumber: 2, pageSize: 2);

        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
    }
}
