using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eCommerce.Test.Common;

/// <summary>
/// Builds an <see cref="AppDbContext"/> backed by the EF Core in-memory provider.
/// Each context gets a unique database name so tests stay fully isolated from one another.
/// </summary>
public static class InMemoryDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // The in-memory provider can't honour relational concerns (transactions, SQL defaults);
            // silence the transaction warning so repository SaveChanges calls don't throw.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new AppDbContext(options);
        // Applies the HasData seed rows (categories, products, reviews, users) for this database.
        context.Database.EnsureCreated();
        return context;
    }
}
