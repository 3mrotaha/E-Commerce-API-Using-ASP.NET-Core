
using eCommerce.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;


public class AccountEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<Account>
{

    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.UseTptMappingStrategy()
            .ToTable(tableName, schema);

        builder.HasKey(x => x.Id);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();
        
        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(200);        
    }
}