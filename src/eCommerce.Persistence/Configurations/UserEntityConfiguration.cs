using eCommerce.Domain.Entities;
using eCommerce.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class UserEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(tableName, schema);

        builder.Property(u => u.IsVipUser)
            .HasDefaultValue(false)
            .ValueGeneratedOnAdd();

        builder.HasMany(u => u.PaymentMethods)
            .WithOne()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Cart)
            .WithOne()
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    
        builder.HasMany(u => u.Orders)
            .WithOne()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ProductReviews)
            .WithOne()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PaymentRecords)
            .WithOne()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
