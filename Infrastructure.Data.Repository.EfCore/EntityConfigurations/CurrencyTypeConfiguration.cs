using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class CurrencyTypeConfiguration : IEntityTypeConfiguration<CurrencyType>
    {
        public void Configure(EntityTypeBuilder<CurrencyType> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Code).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();
            builder.ToTable("CurrencyType");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}