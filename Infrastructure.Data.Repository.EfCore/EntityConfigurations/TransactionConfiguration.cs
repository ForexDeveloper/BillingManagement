using Domain.Core.Entities.TransactionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable("Transaction");
    }
}