using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

/// <summary>
/// Repository tests run against a real (in-memory) <c>AppDbContext</c> so EF query translation and
/// SaveChanges behaviour are genuinely exercised. Carts are not part of the seed data, so counts are exact.
/// </summary>
public class CartRepository_Test
{
    private static CartRepository NewRepo(out eCommerce.Persistence.Data.AppDbContext context)
    {
        context = InMemoryDbContextFactory.Create();
        return new CartRepository(context, NullLogger<CartRepository>.Instance);
    }

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndReturnEntity()
    {
        var repo = NewRepo(out _);
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };

        var added = await repo.AddAsync(cart);

        added.Should().BeSameAs(cart);
        (await repo.GetByIdAsync(cart.Id))!.Id.Should().Be(cart.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo(out _);

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCartsExist_ShouldReturnThem()
    {
        var repo = NewRepo(out _);
        await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });
        await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });

        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenPredicateMatches_ShouldReturnOnlyMatching()
    {
        var repo = NewRepo(out _);
        var userId = Guid.NewGuid();
        await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = userId });
        await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });

        var found = await repo.FindAsync(c => c.UserId == userId);

        found.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo(out _);
        var cart = await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });
        var newUserId = Guid.NewGuid();
        cart!.UserId = newUserId;

        await repo.UpdateAsync(cart);

        (await repo.GetByIdAsync(cart.Id))!.UserId.Should().Be(newUserId);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo(out _);
        var cart = await repo.AddAsync(new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid() });

        await repo.DeleteAsync(cart!);

        (await repo.GetByIdAsync(cart!.Id)).Should().BeNull();
    }
}
