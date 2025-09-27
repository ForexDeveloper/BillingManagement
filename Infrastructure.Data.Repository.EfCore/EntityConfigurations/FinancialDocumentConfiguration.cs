using Domain.Core.Entities.FinancialDocumentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class FinancialDocumentConfiguration : IEntityTypeConfiguration<FinancialDocument>
    {
        public void Configure(EntityTypeBuilder<FinancialDocument> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.CreditAmount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.CashAmount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.PrepaymentAmount).HasColumnType("decimal(32, 10)").IsRequired();


            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();
            builder.Property(p => p.RefundDescription).HasMaxLength(1000);

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("FinancialDocument");
        }
    }
}