using Domain.Core.Entities.CashOutRequestAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class CashOutRequestConfiguration : IEntityTypeConfiguration<CashOutRequest>
{
    public void Configure(EntityTypeBuilder<CashOutRequest> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Description).IsUnicode().HasMaxLength(150);
        builder.Property(p => p.BankTransactionCode).HasMaxLength(100);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(CashOutRequest));
    }
}