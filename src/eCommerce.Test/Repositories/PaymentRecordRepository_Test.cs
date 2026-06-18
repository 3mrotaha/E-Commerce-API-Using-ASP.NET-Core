using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class PaymentRecordRepository_Test
{
    private static PaymentRecordRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<PaymentRecordRepository>.Instance);

    private static PaymentRecord NewRecord(Guid? userId = null, Guid? orderId = null)
        => new() { Id = Guid.NewGuid(), UserId = userId ?? Guid.NewGuid(), OrderId = orderId ?? Guid.NewGuid(), Amount = 50m };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var record = NewRecord();

        await repo.AddAsync(record);

        (await repo.GetByIdAsync(record.Id))!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenRecordsExist_ShouldReturnThem()
    {
        var repo = NewRepo();
        await repo.AddAsync(NewRecord());
        await repo.AddAsync(NewRecord());

        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByOrder_ShouldReturnMatchingRecords()
    {
        var repo = NewRepo();
        var orderId = Guid.NewGuid();
        await repo.AddAsync(NewRecord(orderId: orderId));
        await repo.AddAsync(NewRecord());

        var found = await repo.FindAsync(pr => pr.OrderId == orderId);

        found.Should().ContainSingle().Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var record = await repo.AddAsync(NewRecord());
        record!.Amount = 75m;

        await repo.UpdateAsync(record);

        (await repo.GetByIdAsync(record.Id))!.Amount.Should().Be(75m);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var record = await repo.AddAsync(NewRecord());

        await repo.DeleteAsync(record!);

        (await repo.GetByIdAsync(record!.Id)).Should().BeNull();
    }
}
