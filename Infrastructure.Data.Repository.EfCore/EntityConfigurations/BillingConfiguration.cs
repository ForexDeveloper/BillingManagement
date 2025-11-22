using System.Text.Json;
using Domain.Core.Enums;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BillingAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class BillingConfiguration : IEntityTypeConfiguration<Billing>
{
    public void Configure(EntityTypeBuilder<Billing> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.DueDate).IsRequired();
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.PeriodType).IsRequired();
        builder.Property(p => p.GracePeriod).IsRequired();
        builder.Property(p => p.DebtorId).IsRequired(false);
        builder.Property(p => p.CreditorId).IsRequired(false);
        builder.Property(p => p.PaymentDeadlineDate).IsRequired();
        builder.Property(p => p.ToBusinessIdentityId).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(22).IsRequired();
        builder.Property(p => p.FromBusinessIdentityId).IsRequired();
        builder.Property(p => p.ContractIds).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Transferred).HasDefaultValue(false).IsRequired();
        builder.Property(p => p.Status).HasDefaultValue(BillingStatus.Issued).IsRequired();
        builder.Property(p => p.AdditionsDescription).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.DeductionsDescription).HasMaxLength(1000).IsRequired(false);
        builder.Property(p => p.Amount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.AdditionsAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.DeductionsAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousDebitAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousCreditAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousPenaltyAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.TieredTransactionsAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();
        builder.Property(e => e.RowVersion).IsRowVersion();
        builder.HasIndex(p => p.Code).IsUnique();

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        builder
            .Property(b => b.TieredCalculatedLevels).HasMaxLength(4000).IsRequired(false)
            .HasConversion(
                x => JsonSerializer.Serialize(x, options),
                x => JsonSerializer.Deserialize<List<TieredCalculatedLevel>>(x, options))
            .IsUnicode();

        builder.HasOne(p => p.Debtor)
            .WithMany(p => p.DebtorChildren)
            .HasForeignKey(p => p.DebtorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Creditor)
            .WithMany(p => p.CreditorChildren)
            .HasForeignKey(p => p.CreditorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Payments)
           .WithOne(b => b.Billing)
           .HasForeignKey(p => p.BillingId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(Billing));
    }
}