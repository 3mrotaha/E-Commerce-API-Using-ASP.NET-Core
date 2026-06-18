using eCommerce.Domain.Entities;
using eCommerce.Domain.Enums;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class OrderRepository_Test
{
    private static OrderRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<OrderRepository>.Instance);

    private static Order NewOrder(Guid? userId = null)
        => new() { Id = Guid.NewGuid(), UserId = userId ?? Guid.NewGuid(), OrderState = OrderState.PENDING };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var order = NewOrder();

        await repo.AddAsync(order);

        (await repo.GetByIdAsync(order.Id))!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenOrdersExist_ShouldReturnThem()
    {
        var repo = NewRepo();
        await repo.AddAsync(NewOrder());
        await repo.AddAsync(NewOrder());

        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByUser_ShouldReturnMatchingOrders()
    {
        var repo = NewRepo();
        var userId = Guid.NewGuid();
        await repo.AddAsync(NewOrder(userId));
        await repo.AddAsync(NewOrder(Guid.NewGuid()));

        var found = await repo.FindAsync(o => o.UserId == userId);

        found.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var order = await repo.AddAsync(NewOrder());
        order!.OrderState = OrderState.SHIPPED;

        await repo.UpdateAsync(order);

        (await repo.GetByIdAsync(order.Id))!.OrderState.Should().Be(OrderState.SHIPPED);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var order = await repo.AddAsync(NewOrder());

        await repo.DeleteAsync(order!);

        (await repo.GetByIdAsync(order!.Id)).Should().BeNull();
    }
}
