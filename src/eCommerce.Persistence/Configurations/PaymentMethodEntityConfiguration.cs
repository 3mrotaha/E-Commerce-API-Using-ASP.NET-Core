using eCommerce.Domain.Entities;
using eCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class PaymentMethodEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.UseTptMappingStrategy().ToTable(tableName, schema);

        builder.HasKey(x => x.Id);
            
        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();
    }
}
