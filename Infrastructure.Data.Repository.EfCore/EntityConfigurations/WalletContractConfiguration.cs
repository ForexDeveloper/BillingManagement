using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class WalletContractConfiguration : IEntityTypeConfiguration<WalletContract>
    {
        public void Configure(EntityTypeBuilder<WalletContract> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.StartDate).IsRequired();
            builder.Property(p => p.EndDate).IsRequired(false);
            builder.Property(p => p.Status).IsRequired();


            builder.ToTable("WalletContract");
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}