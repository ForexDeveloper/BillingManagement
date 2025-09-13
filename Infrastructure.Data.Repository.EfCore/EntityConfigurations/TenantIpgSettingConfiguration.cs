using Domain.Core.Entities.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class TenantIpgSettingConfiguration : IEntityTypeConfiguration<TenantIpgSetting>
{
    public void Configure(EntityTypeBuilder<TenantIpgSetting> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.IpgType).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.Title).IsRequired().HasMaxLength(256);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(TenantIpgSetting));
    }
}