using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class OrderItemRepository_Test
{
    private static OrderItemRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<OrderItemRepository>.Instance);

    private static OrderItem NewItem(Guid? orderId = null)
        => new() { OrderId = orderId ?? Guid.NewGuid(), UserId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 5m };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndAssignIdentityKey()
    {
        var repo = NewRepo();

        var added = await repo.AddAsync(NewItem());

        added!.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCalled_ShouldReturnNullBecauseKeyIsInt()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByOrder_ShouldReturnMatchingItems()
    {
        var repo = NewRepo();
        var orderId = Guid.NewGuid();
        await repo.AddAsync(NewItem(orderId));
        await repo.AddAsync(NewItem(Guid.NewGuid()));

        var found = await repo.FindAsync(oi => oi.OrderId == orderId);

        found.Should().ContainSingle().Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var item = await repo.AddAsync(NewItem());
        item!.Quantity = 11;

        await repo.UpdateAsync(item);

        (await repo.FindAsync(oi => oi.Id == item.Id)).Single().Quantity.Should().Be(11);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var item = await repo.AddAsync(NewItem());

        await repo.DeleteAsync(item!);

        (await repo.FindAsync(oi => oi.Id == item!.Id)).Should().BeEmpty();
    }
}
