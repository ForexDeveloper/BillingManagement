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
            builder.Property(p => p.FullName).HasMaxLength(250).IsRequired();
            builder.Property(p => p.NationalId).HasMaxLength(10).IsRequired();
            builder.Property(p => p.Mobile).HasMaxLength(20).IsRequired();

            builder.ToTable("Customer");

        }
    }
}