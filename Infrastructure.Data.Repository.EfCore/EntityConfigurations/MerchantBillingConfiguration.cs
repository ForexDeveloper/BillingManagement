using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantBillingAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class MerchantBillingConfiguration : IEntityTypeConfiguration<MerchantBilling>
{
    public void Configure(EntityTypeBuilder<MerchantBilling> builder)
    {
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.ParentId).IsRequired(false);
        builder.Property(p => p.Code).HasMaxLength(100).IsRequired();
        builder.Property(p => p.AdditionsDescription).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.DeductionsDescription).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.Additions).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.Deductions).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.CurrentPeriodFinalCommission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.RefundedTransactionsCommission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.CurrentPeriodPurchaseTransactions).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousPeriodRefundedTransactions).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();

        builder.HasOne(p => p.Parent)
            .WithOne()
            .HasForeignKey<MerchantBilling>(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(MerchantBilling));
    }
}