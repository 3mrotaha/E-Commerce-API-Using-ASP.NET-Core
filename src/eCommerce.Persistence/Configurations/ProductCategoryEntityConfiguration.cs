using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class ProductCategoryEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable(tableName, schema)
            .HasQueryFilter(pc => !pc.IsDeleted);
        builder.HasKey(x => x.Id);

        builder.Property(pc => pc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pc => pc.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasIndex(pc => pc.Name); // index on Name for faster search by category name
    }
}
