using Domain.Core.Entities.WalletAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class LoanWalletConfiguration : IEntityTypeConfiguration<LoanWallet>
    {
        public void Configure(EntityTypeBuilder<LoanWallet> builder)
        {
            builder.Property(p => p.OperationalFee).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.InitialAmount).HasColumnType("decimal(32, 10)").IsRequired();

            builder.ToTable("LoanWallet");
        }
    }
}