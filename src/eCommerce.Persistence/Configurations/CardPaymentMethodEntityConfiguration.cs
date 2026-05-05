using eCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Persistence.Configurations;

public class CardPaymentMethodEntityConfiguration(string schema, string tableName) : IEntityTypeConfiguration<CardPaymentMethod>
{
    public void Configure(EntityTypeBuilder<CardPaymentMethod> builder)
    {
        builder.ToTable(tableName, schema);

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(c => c.CardExpiryDate)
            .IsRequired();

        builder.Property(c => c.CardHolderName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CVV)
            .IsRequired()
            .HasMaxLength(4);
    }
}
