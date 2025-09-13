using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations;
public class WalletConfigurationConfiguration : IEntityTypeConfiguration<WalletConfiguration>
{
    public void Configure(EntityTypeBuilder<WalletConfiguration> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.WalletTypeId).IsRequired();
        builder.Property(p => p.ProjectManagerId).IsRequired(false);
        builder.Property(p => p.WalletTypeId).IsRequired();
        builder.Property(p => p.MaxWallet).HasColumnType("decimal(32, 10)").IsRequired();
        builder.Property(p => p.MaxTotalCredit).HasColumnType("decimal(32, 10)").IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.PrepaymentMaxPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.PrepaymentMinPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.PrepaymentMaxAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.PrepaymentMinAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.InterestPeriodMaxPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.InterestPeriodMinPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.InterestPeriodMaxAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.InterestPeriodMinAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.PenaltyPeriodMaxPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.PenaltyPeriodMinPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.PenaltyPeriodMaxAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.PenaltyPeriodMinAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.WaiverPeriodMaxPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.WaiverPeriodMinPercent).HasColumnType("decimal(6, 3)");
        builder.Property(p => p.WaiverPeriodMaxAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.WaiverPeriodMinAmount).HasColumnType("decimal(32, 10)");
        builder.Property(p => p.MaxInstallments).HasMaxLength(500);
        builder.Property(p => p.Title).HasMaxLength(250);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(WalletConfiguration));


    }
}