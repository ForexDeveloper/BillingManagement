using Domain.Core.Entities.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();
            builder.Property(p => p.BackgroundColor1).HasMaxLength(50).IsRequired(false);
            builder.Property(p => p.BackgroundColor2).HasMaxLength(50).IsRequired(false);
            builder.Property(p => p.TextColor).HasMaxLength(50).IsRequired(false);
            builder.Property(p => p.Description).HasMaxLength(1000).IsRequired(false);
            builder.Property(p => p.Link).HasMaxLength(500).IsRequired(false);
            builder.Property(p => p.MaxTotalCredit).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.ReservedCredit).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.AssignedCredit).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.MaxWallet).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.MaxDailyWithdrawal).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.MaxDailyDeposit).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.TermsAndConditions).HasDefaultValue(string.Empty)
                .IsRequired()
                .HasColumnType("nvarchar(max)")
                .IsUnicode(true);
            builder.Property(p => p.RowVersion).IsRowVersion();

            builder.ToTable("Plan");
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}