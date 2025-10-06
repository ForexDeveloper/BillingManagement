using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantBillingAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class MerchantBillingConfiguration : IEntityTypeConfiguration<MerchantBilling>
{
    public void Configure(EntityTypeBuilder<MerchantBilling> builder)
    {
        builder.Property(p => p.PurchaseTransactionsAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.RefundedTransactionsAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PurchaseTransactionsCommission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.RefundedTransactionsCommission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PurchaseTransactionsCalculatedCommission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();

        builder.ToTable(nameof(MerchantBilling));
    }
}