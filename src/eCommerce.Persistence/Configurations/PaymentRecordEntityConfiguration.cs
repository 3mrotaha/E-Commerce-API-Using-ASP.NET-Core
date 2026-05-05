using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class PaymentRecordEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable(tableName, schema);
        builder.HasKey(x => x.Id);

        builder.Property(pr => pr.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(pr => pr.Order)
            .WithMany()
            .HasForeignKey(pr => pr.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(pr => pr.PaymentMethod)
            .WithMany()
            .HasForeignKey(pr => pr.PaymentMethodId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(pr => pr.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();
    }
}
