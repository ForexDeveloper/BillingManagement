using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;
public class TenantMerchantContractConfiguration : IEntityTypeConfiguration<TenantMerchantContract>
{
    public void Configure(EntityTypeBuilder<TenantMerchantContract> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.MerchantId).IsRequired();
        builder.Property(p => p.EnamadLink).HasMaxLength(250).IsRequired(false);
        builder.Property(p => p.InternetBusinessLicenseLink).HasMaxLength(250).IsRequired(false);
        builder.Property(p => p.ContractNumber).HasMaxLength(30).IsRequired();
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.EndDate).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.SettlementType).IsRequired();
        builder.Property(p => p.IsCommissionExchanged).IsRequired();
        builder.Property(p => p.CommissionDeductionMethodType).IsRequired(false);
        builder.Property(p => p.InterestPercentage).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.InterestReferenceTypes).HasMaxLength(100).IsRequired(false);
        builder.Property(p => p.BillingPeriodType).IsRequired();
        builder.Property(p => p.BillingPeriod).IsRequired();
        builder.Property(p => p.PaymentMethodType).IsRequired(true);
        builder.Property(p => p.GuaranteeType).IsRequired(false);
        builder.Property(p => p.GuaranteeDescription).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.CommissionCalculationType).IsRequired();
        builder.Property(p => p.FixedAmountCommission).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.FixedPercentageCommission).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.CommissionReferenceTypes).HasMaxLength(100).IsRequired(false);
        builder.Property(p => p.TransactionMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.TransactionMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.PeriodMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.PeriodMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        builder
            .Property(b => b.TieredCommissions).HasMaxLength(4000)
            .HasConversion(
                x => JsonSerializer.Serialize(x, options),
                x => JsonSerializer.Deserialize<List<TieredCommission>>(x, options))
            .IsUnicode();

        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.ToTable(nameof(TenantMerchantContract));
    }
}
