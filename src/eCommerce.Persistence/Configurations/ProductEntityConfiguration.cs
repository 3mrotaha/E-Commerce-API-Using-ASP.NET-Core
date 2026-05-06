using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class ProductEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(tableName, schema)            
            .HasQueryFilter(p => !p.IsDeleted);            


        builder.HasKey(x => x.Id);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasMaxLength(10000)
            .IsRequired(false);
        
        builder.Property(p => p.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.QuantityInStock)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();
        
        builder.HasIndex(p => new { p.Name, p.CategoryId, p.UnitPrice, p.Id })
                .HasDatabaseName("IX_Products_Name_CategoryId_UnitPrice_Id"); // composite index on Name, CategoryId and UnitPrice for faster search and filtering by name, category and price

        builder.HasIndex(p => new {p.Name, p.UnitPrice, p.Id})
                .HasDatabaseName("IX_Products_Name_UnitPrice_Id"); // index on Name and UnitPrice for faster search by name and price range

        builder.HasIndex(p => new {p.CategoryId, p.UnitPrice, p.Id})
                .HasDatabaseName("IX_Products_CategoryId_UnitPrice_Id"); // index on CategoryId and UnitPrice for faster filtering by category and price range

        builder.HasIndex(p => new {p.Name, p.Id })
                .HasDatabaseName("IX_Products_Name_Id"); // index on Name for faster search by name

        builder.HasIndex(p => new {p.CategoryId, p.Id})
                .HasDatabaseName("IX_Products_CategoryId_Id"); // index on CategoryId for faster filtering by category

        builder.HasIndex(p => new {p.UnitPrice, p.Id})
                .HasDatabaseName("IX_Products_UnitPrice_Id"); // index on UnitPrice for faster price range queries    
    }
}
