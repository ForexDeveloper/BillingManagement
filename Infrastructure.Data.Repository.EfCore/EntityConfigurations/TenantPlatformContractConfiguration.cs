using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class TenantPlatformContractConfiguration : IEntityTypeConfiguration<TenantPlatformContract>
{
    public void Configure(EntityTypeBuilder<TenantPlatformContract> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.ContractNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.EndDate).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.FeeCalculationType).IsRequired();
        builder.Property(p => p.CommissionCalculationType).IsRequired();
        builder.Property(p => p.FixedAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.FixedAmountCommission).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.FixedPercentageCommission).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.CommissionReferenceTypes).HasMaxLength(100).IsRequired(false);
        builder.Property(p => p.TransactionMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.TransactionMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.PeriodMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.PeriodMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
        builder.Property(p => p.BillingPeriodType).IsRequired();
        builder.Property(p => p.BillingPeriod).IsRequired();
        builder.Property(p => p.GracePeriod).IsRequired(false);
        builder.Property(p => p.PenaltyPercent).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.TenantIpgSettingId).IsRequired();
        builder.Property(p => p.Status).IsRequired();

        builder.HasMany(x => x.Providers)
            .WithOne(x => x.TenantPlatformContract)
            .HasForeignKey(x => x.TenantPlatformContractId);

        builder.HasMany(x => x.Facilitators)
            .WithOne(x => x.TenantPlatformContract)
            .HasForeignKey(x => x.TenantPlatformContractId);

        builder.HasOne(x => x.TenantIpgSettings)
            .WithMany(x => x.TenantPlatformContracts)
            .HasForeignKey(x => x.TenantIpgSettingId);

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
        builder.ToTable(nameof(TenantPlatformContract));

    }
}
