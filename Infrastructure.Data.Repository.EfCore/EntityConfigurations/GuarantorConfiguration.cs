using Domain.Core.Entities.GuarantorAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class GuarantorConfiguration : IEntityTypeConfiguration<Guarantor>
    {
        public void Configure(EntityTypeBuilder<Guarantor> builder)
        {
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Type).IsRequired();

            builder.ToTable("Guarantor");

        }
    }
}