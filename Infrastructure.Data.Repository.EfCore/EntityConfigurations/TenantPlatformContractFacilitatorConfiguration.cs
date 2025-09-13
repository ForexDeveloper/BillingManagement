using Domain.Core.Entities.TenantPlatformContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class TenantPlatformContractFacilitatorConfiguration : IEntityTypeConfiguration<TenantPlatformContractFacilitator>
{
    public void Configure(EntityTypeBuilder<TenantPlatformContractFacilitator> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.FacilitatorId).IsRequired();
        builder.Property(p => p.FixedAmountCommissionPercentage).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.TransactionsCommissionPercentage).HasColumnType("decimal(6, 3)").IsRequired(false);
        builder.Property(p => p.PaymentMethodType).IsRequired(false);

        builder.HasOne(x => x.TenantPlatformContract)
            .WithMany(x => x.Facilitators)
            .HasForeignKey(x => x.TenantPlatformContractId);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable("TenantPlatformContractFacilitator");
    }
}