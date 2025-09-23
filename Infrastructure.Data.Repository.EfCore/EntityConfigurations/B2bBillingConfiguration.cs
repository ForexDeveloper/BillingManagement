using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.B2bBillingAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class B2bBillingConfiguration : IEntityTypeConfiguration<B2bBilling>
{
    public void Configure(EntityTypeBuilder<B2bBilling> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.EndDate).IsRequired();
        builder.Property(p => p.DueDate).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.PeriodType).IsRequired();
        builder.Property(p => p.GracePeriod).IsRequired();
        builder.Property(p => p.ToBusinessIdentityId).IsRequired();
        builder.Property(p => p.FromBusinessIdentityId).IsRequired();
        builder.Property(p => p.Status).HasDefaultValue(BillingStatus.Issued).IsRequired();
        builder.Property(p => p.Amount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousDebitAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousCreditAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
        builder.Property(p => p.PreviousPenaltyAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();

        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}