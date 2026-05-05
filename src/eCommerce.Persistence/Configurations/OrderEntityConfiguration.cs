using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class OrderEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(tableName, schema);
        builder.HasKey(x => x.Id);

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(o => o.DiscountValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();
    }
}
