using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class ProductReviewEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable(tableName, schema)
            .HasQueryFilter(pr => !pr.IsDeleted);

        builder.HasKey(x => x.Id);        
        
        builder.Property(c => c.Stars)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.HasOne(pr => pr.Product)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(pr => pr.ProductId)            
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(c => c.Comment)
            .HasMaxLength(1000)
            .IsRequired(false);        

        builder.Property(pr => pr.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();

        builder.Property(pr => pr.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();
    }
}
