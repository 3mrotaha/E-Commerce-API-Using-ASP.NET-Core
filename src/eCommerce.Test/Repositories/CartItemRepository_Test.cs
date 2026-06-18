using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class CartItemRepository_Test
{
    private static CartItemRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<CartItemRepository>.Instance);

    private static CartItem NewItem(Guid? cartId = null)
        => new() { CartId = cartId ?? Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 5m };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndAssignIdentityKey()
    {
        var repo = NewRepo();
        var item = NewItem();

        var added = await repo.AddAsync(item);

        added!.Id.Should().BeGreaterThan(0); // int identity assigned by the store
    }

    [Fact]
    public async Task GetByIdAsync_WhenCalled_ShouldReturnNullBecauseKeyIsInt()
    {
        var repo = NewRepo();

        // CartItem uses an int PK; the Guid-based generic contract is intentionally unsupported.
        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByCart_ShouldReturnMatchingItems()
    {
        var repo = NewRepo();
        var cartId = Guid.NewGuid();
        await repo.AddAsync(NewItem(cartId));
        await repo.AddAsync(NewItem(Guid.NewGuid()));

        var found = await repo.FindAsync(ci => ci.CartId == cartId);

        found.Should().ContainSingle().Which.CartId.Should().Be(cartId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var item = await repo.AddAsync(NewItem());
        item!.Quantity = 9;

        await repo.UpdateAsync(item);

        var found = await repo.FindAsync(ci => ci.Id == item.Id);
        found.Single().Quantity.Should().Be(9);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var item = await repo.AddAsync(NewItem());

        await repo.DeleteAsync(item!);

        (await repo.FindAsync(ci => ci.Id == item!.Id)).Should().BeEmpty();
    }
}
