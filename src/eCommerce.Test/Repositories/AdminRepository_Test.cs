using eCommerce.Domain.Identity;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class AdminRepository_Test
{
    private static AdminRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<AdminRepository>.Instance);

    private static Admin NewAdmin(string suffix, bool activated = false)
        => new()
        {
            Id = Guid.NewGuid(),
            FullName = "Admin Name",
            UserName = $"admin-{suffix}",
            Email = $"admin-{suffix}@example.com",
            IsActivated = activated
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var admin = NewAdmin("add");

        await repo.AddAsync(admin);

        (await repo.GetByIdAsync(admin.Id))!.UserName.Should().Be(admin.UserName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenAdminsExist_ShouldReturnOnlyAdmins()
    {
        var repo = NewRepo();
        await repo.AddAsync(NewAdmin("a"));
        await repo.AddAsync(NewAdmin("b"));

        // No admins are seeded, so the count is exact.
        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByActivation_ShouldReturnMatchingAdmins()
    {
        var repo = NewRepo();
        var active = NewAdmin("active", activated: true);
        await repo.AddAsync(active);
        await repo.AddAsync(NewAdmin("inactive", activated: false));

        var found = await repo.FindAsync(a => a.IsActivated);

        found.Should().ContainSingle().Which.Id.Should().Be(active.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var admin = await repo.AddAsync(NewAdmin("update"));
        admin!.IsActivated = true;

        await repo.UpdateAsync(admin);

        (await repo.GetByIdAsync(admin.Id))!.IsActivated.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var admin = await repo.AddAsync(NewAdmin("delete"));

        await repo.DeleteAsync(admin!);

        (await repo.GetByIdAsync(admin!.Id)).Should().BeNull();
    }
}
