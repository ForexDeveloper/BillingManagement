using Domain.Core.Entities.FinancialDocumentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations;

public class FinancialDocumentPaymentConfiguration : IEntityTypeConfiguration<FinancialDocumentPayment>
{
    public void Configure(EntityTypeBuilder<FinancialDocumentPayment> builder)
    {
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();
        builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable("FinancialDocumentPayment");
    }
}