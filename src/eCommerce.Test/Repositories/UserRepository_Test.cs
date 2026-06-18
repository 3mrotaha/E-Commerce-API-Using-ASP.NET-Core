using eCommerce.Domain.Identity;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

/// <summary>
/// Users are part of the seed (2 rows) and mapped via TPT under the Accounts set; the in-memory
/// provider flattens the hierarchy so OfType&lt;User&gt; queries still work.
/// </summary>
public class UserRepository_Test
{
    private static UserRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<UserRepository>.Instance);

    private static User NewUser(string suffix)
        => new()
        {
            Id = Guid.NewGuid(),
            FullName = "Full Name",
            UserName = $"user-{suffix}",
            Email = $"user-{suffix}@example.com",
            IsVipUser = false,
            Points = 0
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var user = NewUser("add");

        await repo.AddAsync(user);

        (await repo.GetByIdAsync(user.Id))!.UserName.Should().Be(user.UserName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldIncludeSeededAndAddedUsers()
    {
        var repo = NewRepo();
        var user = await repo.AddAsync(NewUser("all"));

        var all = await repo.GetAllAsync();

        all.Should().Contain(u => u.Id == user!.Id);
        all.Count().Should().BeGreaterThan(2); // 2 seeded + the one added
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByPoints_ShouldReturnMatchingUsers()
    {
        var repo = NewRepo();
        var user = NewUser("find");
        user.Points = 9999; // distinctive value not used by the seed
        await repo.AddAsync(user);

        var found = await repo.FindAsync(u => u.Points == 9999);

        found.Should().ContainSingle().Which.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var user = await repo.AddAsync(NewUser("update"));
        user!.IsVipUser = true;

        await repo.UpdateAsync(user);

        (await repo.GetByIdAsync(user.Id))!.IsVipUser.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var user = await repo.AddAsync(NewUser("delete"));

        await repo.DeleteAsync(user!);

        (await repo.GetByIdAsync(user!.Id)).Should().BeNull();
    }
}
