using Domain.Core.Entities.WalletAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class BnplWalletConfiguration : IEntityTypeConfiguration<BnplWallet>
    {
        public void Configure(EntityTypeBuilder<BnplWallet> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.HasQueryFilter(p => !p.IsDeleted);
            builder.ToTable("BnplWallet");
        }
    }
}