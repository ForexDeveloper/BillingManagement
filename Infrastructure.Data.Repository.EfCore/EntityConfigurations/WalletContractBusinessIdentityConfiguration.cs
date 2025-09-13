using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class WalletContractBusinessIdentityConfiguration : IEntityTypeConfiguration<WalletContractBusinessIdentity>
{
    public void Configure(EntityTypeBuilder<WalletContractBusinessIdentity> builder)
    {

        builder.ToTable("WalletContractBusinessIdentity");

    }
}