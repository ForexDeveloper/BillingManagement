using Domain.Core.Entities.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
    {
        public void Configure(EntityTypeBuilder<Merchant> builder)
        {
            builder.Property(p => p.Type).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();
            builder.ToTable("Merchant");

        }
    }
}