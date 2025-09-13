using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Core.Entities.MerchantInstallmentAggregate;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class MerchantInstallmentConfiguration : IEntityTypeConfiguration<MerchantInstallment>
{
    public void Configure(EntityTypeBuilder<MerchantInstallment> builder)
    {
        builder.Property(p => p.TenantMerchantContractId).IsRequired();

        builder.HasOne(p => p.TenantMerchantContract)
            .WithMany()
            .HasForeignKey(p => p.TenantMerchantContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(MerchantInstallment));
    }
}