using Domain.Core.Entities.WalletAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class CashWalletConfiguration : IEntityTypeConfiguration<CashWallet>
{
    public void Configure(EntityTypeBuilder<CashWallet> builder)
    {
        builder.ToTable("CashWallet");
    }
}