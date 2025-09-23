using Domain.Core.Entities.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();
            builder.Property(p => p.BrandName).HasMaxLength(250);
            builder.Property(p => p.CreditProjectName).HasMaxLength(250);
            builder.Property(p => p.InternalProjectManagerName).HasMaxLength(500);

            builder.ToTable("Tenant");
        }
    }
}