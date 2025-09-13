using Domain.Core.Entities.BillingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations;

public class BillingPaymentConfiguration : IEntityTypeConfiguration<BillingPayment>
{
    public void Configure(EntityTypeBuilder<BillingPayment> builder)
    {
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();
        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable("BillingPayment");
    }
}