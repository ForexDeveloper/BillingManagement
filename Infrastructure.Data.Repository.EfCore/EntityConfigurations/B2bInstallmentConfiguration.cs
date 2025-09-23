using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.B2bInstallmentAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class B2bInstallmentConfiguration : IEntityTypeConfiguration<B2bInstallment>
{
    public void Configure(EntityTypeBuilder<B2bInstallment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.Number).IsRequired();
        builder.Property(p => p.DueDate).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.BillingId).IsRequired(false);
        builder.Property(p => p.ToBusinessIdentityId).IsRequired();
        builder.Property(p => p.FromBusinessIdentityId).IsRequired();
        builder.Property(p => p.Amount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();

        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.FromBusinessIdentity)
            .WithMany()
            .HasForeignKey(p => p.FromBusinessIdentityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ToBusinessIdentity)
            .WithMany()
            .HasForeignKey(p => p.ToBusinessIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}