using Domain.Core.Entities.FacilitatorAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class FacilitatorConfiguration : IEntityTypeConfiguration<Facilitator>
    {
        public void Configure(EntityTypeBuilder<Facilitator> builder)
        {
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Type).IsRequired();

            builder.ToTable("Facilitator");

        }
    }
}