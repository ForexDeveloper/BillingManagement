using Domain.Core.Entities.FinancierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class FinancierConfiguration : IEntityTypeConfiguration<Financier>
    {
        public void Configure(EntityTypeBuilder<Financier> builder)
        {
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Type).IsRequired();

            builder.ToTable("Financier");

        }
    }
}