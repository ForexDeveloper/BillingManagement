using Domain.Core.Entities.TenantPlatformContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class TenantPlatformContractProviderConfiguration : IEntityTypeConfiguration<TenantPlatformContractProvider>
{
    public void Configure(EntityTypeBuilder<TenantPlatformContractProvider> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.TenantPlatformContractProviders)
            .HasForeignKey(x => x.ProviderId);

        builder.HasOne(x => x.TenantPlatformContract)
            .WithMany(x => x.Providers)
            .HasForeignKey(x => x.TenantPlatformContractId);

        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.ToTable("TenantPlatformContractProvider");

    }
}