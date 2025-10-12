using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Infrastructure.Data.Repository.EfCore.Constants;
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

            builder.Property(p => p.Amount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
            builder.Property(p => p.Commission).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
            builder.Property(p => p.CashAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
            builder.Property(p => p.CreditAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();
            builder.Property(p => p.PrepaymentAmount).HasColumnType(ColumnTypes.DECIMAL_32_10).IsRequired();


            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();
            builder.Property(p => p.RefundDescription).HasMaxLength(1000);

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("FinancialDocument");
        }
    }
}