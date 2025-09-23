using Domain.Core.Entities.Shared;
using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class WalletContractGuarantorConfiguration : IEntityTypeConfiguration<WalletContractGuarantor>
    {
        public void Configure(EntityTypeBuilder<WalletContractGuarantor> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.PortionTypes).IsRequired();

            builder.HasOne(x => x.WalletContract)
                .WithMany(x => x.WalletContractGuarantors)
                .HasForeignKey(x => x.WalletContractId);

            builder.HasOne(x => x.Guarantor)
                .WithMany(x => x.WalletContractGuarantor)
                .HasForeignKey(x => x.GuarantorId);

            builder.Property(p => p.CommissionCalculationType).IsRequired(false);
            builder.Property(p => p.FixedAmountCommission).HasColumnType("decimal(32, 10)").IsRequired(false);
            builder.Property(p => p.FixedPercentageCommission).HasColumnType("decimal(6, 3)").IsRequired(false);
            builder.Property(p => p.PortionTypes).HasMaxLength(100).IsRequired(false);
            builder.Property(p => p.TransactionMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
            builder.Property(p => p.TransactionMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
            builder.Property(p => p.PeriodMinCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);
            builder.Property(p => p.PeriodMaxCommissionAmount).HasColumnType("decimal(32, 10)").IsRequired(false);

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            builder
                .Property(b => b.TieredCommissions).HasMaxLength(4000)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, options),
                    x => JsonSerializer.Deserialize<List<TieredCommission>>(x, options))
                .IsUnicode();

            builder.ToTable("WalletContractGuarantor");
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}