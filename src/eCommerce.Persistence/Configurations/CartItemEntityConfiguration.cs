using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class CartItemEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(tableName, schema)
             .HasQueryFilter(ci => !ci.IsDeleted);
        builder.HasKey(x => x.Id);

        builder.HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();
    }
}
