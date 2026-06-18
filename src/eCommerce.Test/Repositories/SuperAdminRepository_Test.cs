using eCommerce.Domain.Identity;
using eCommerce.Persistence.Repositories;
using eCommerce.Test.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace eCommerce.Test.Repositories;

public class SuperAdminRepository_Test
{
    private static SuperAdminRepository NewRepo()
        => new(InMemoryDbContextFactory.Create(), NullLogger<SuperAdminRepository>.Instance);

    private static SuperAdmin NewSuperAdmin(string suffix)
        => new()
        {
            Id = Guid.NewGuid(),
            FullName = "Super Admin",
            UserName = $"super-{suffix}",
            Email = $"super-{suffix}@example.com"
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldPersistAndBeRetrievableById()
    {
        var repo = NewRepo();
        var superAdmin = NewSuperAdmin("add");

        await repo.AddAsync(superAdmin);

        (await repo.GetByIdAsync(superAdmin.Id))!.UserName.Should().Be(superAdmin.UserName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ShouldReturnNull()
    {
        var repo = NewRepo();

        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenSuperAdminsExist_ShouldReturnOnlySuperAdmins()
    {
        var repo = NewRepo();
        await repo.AddAsync(NewSuperAdmin("a"));
        await repo.AddAsync(NewSuperAdmin("b"));

        (await repo.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_WhenFilteredByEmail_ShouldReturnMatchingSuperAdmin()
    {
        var repo = NewRepo();
        var superAdmin = NewSuperAdmin("find");
        await repo.AddAsync(superAdmin);
        await repo.AddAsync(NewSuperAdmin("other"));

        var found = await repo.FindAsync(s => s.Email == superAdmin.Email);

        found.Should().ContainSingle().Which.Id.Should().Be(superAdmin.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ShouldPersistChanges()
    {
        var repo = NewRepo();
        var superAdmin = await repo.AddAsync(NewSuperAdmin("update"));
        superAdmin!.FullName = "Renamed";

        await repo.UpdateAsync(superAdmin);

        (await repo.GetByIdAsync(superAdmin.Id))!.FullName.Should().Be("Renamed");
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldRemoveEntity()
    {
        var repo = NewRepo();
        var superAdmin = await repo.AddAsync(NewSuperAdmin("delete"));

        await repo.DeleteAsync(superAdmin!);

        (await repo.GetByIdAsync(superAdmin!.Id)).Should().BeNull();
    }
}
