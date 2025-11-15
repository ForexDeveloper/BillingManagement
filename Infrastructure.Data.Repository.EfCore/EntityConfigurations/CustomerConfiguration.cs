using Domain.Core.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.FullName).HasMaxLength(250);
            builder.Property(p => p.NationalId).HasMaxLength(10);
            builder.Property(p => p.Mobile).HasMaxLength(20);
            builder.Property(p => p.UniqueIdentifier).HasMaxLength(50);

            builder.ToTable("Customer");

        }
    }
}