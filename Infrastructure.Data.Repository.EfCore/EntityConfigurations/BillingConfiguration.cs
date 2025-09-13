using Domain.Core.Entities.BillingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class BillingConfiguration : IEntityTypeConfiguration<Billing>
    {
        public void Configure(EntityTypeBuilder<Billing> builder)
        {
            builder.Property(p => p.Id).IsRequired();

            builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.PreviousDebitAmount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.PreviousCreditAmount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.PreviousPenaltyAmount).HasColumnType("decimal(32, 10)").IsRequired();

            builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

            builder.HasMany(x => x.BillingPayments)
                .WithOne(x => x.Billing)
                .HasForeignKey(x => x.BillingId);

            builder.Property(e => e.RowVersion)
                   .IsRowVersion();

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("Billing");
        }
    }
}