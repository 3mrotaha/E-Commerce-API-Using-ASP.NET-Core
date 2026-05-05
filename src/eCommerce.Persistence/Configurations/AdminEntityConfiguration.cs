using eCommerce.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class AdminEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable(tableName, schema);

        builder.Property(a => a.IsActivated)
            .HasDefaultValue(false);

        
    }
}
