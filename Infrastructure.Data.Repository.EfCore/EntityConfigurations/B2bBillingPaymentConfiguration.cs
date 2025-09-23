using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.B2bBillingPaymentAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class B2bBillingPaymentConfiguration : IEntityTypeConfiguration<B2bBillingPayment>
{
    public void Configure(EntityTypeBuilder<B2bBillingPayment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.Amount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.HasOne(p => p.Billing)
            .WithMany()
            .HasForeignKey(p => p.BillingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}