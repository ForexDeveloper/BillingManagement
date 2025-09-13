using Domain.Core.Entities.BankAccountAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.AccountNumber).IsRequired(false).HasMaxLength(24);
        builder.Property(p => p.OwnerFullName).IsRequired(false).HasMaxLength(100);
        builder.Property(p => p.BankId).IsRequired(false);
        builder.Property(p => p.Iban).IsRequired(false).HasMaxLength(26);
        builder.Property(p => p.IsDefault).IsRequired().HasDefaultValue(false);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(BankAccount));
    }
}