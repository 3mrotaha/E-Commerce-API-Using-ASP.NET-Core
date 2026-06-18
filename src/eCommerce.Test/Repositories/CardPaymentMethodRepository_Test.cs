using eCommerce.Domain.Entities;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class CardPaymentMethodRepository_Test
{
    private static CardPaymentMethodRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<CardPaymentMethodRepository>.Instance);

    private static CardPaymentMethod NewCard(Guid? userId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            CardNumber = "4111111111111111",
            CVV = "123",
            CardHolderName = "Jane Doe",
            CardExpiryDate = DateTime.UtcNow.AddYears(2)
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var card = NewCard();

        await repo.AddAsync(card);

        (await repo.GetByIdAsync(card.Id))!.CardHolderName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCardsExist_ShouldReturnThem()
    {
        var repo = NewRepo();
        await repo.AddAsync(NewCard());
        await repo.AddAsync(NewCard());

        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByUser_ShouldReturnMatchingCards()
    {
        var repo = NewRepo();
        var userId = Guid.NewGuid();
        await repo.AddAsync(NewCard(userId));
        await repo.AddAsync(NewCard(Guid.NewGuid()));

        var found = await repo.FindAsync(c => c.UserId == userId);

        found.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var card = await repo.AddAsync(NewCard());
        card!.CardHolderName = "John Smith";

        await repo.UpdateAsync(card);

        (await repo.GetByIdAsync(card.Id))!.CardHolderName.Should().Be("John Smith");
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var card = await repo.AddAsync(NewCard());

        await repo.DeleteAsync(card!);

        (await repo.GetByIdAsync(card!.Id)).Should().BeNull();
    }
}
