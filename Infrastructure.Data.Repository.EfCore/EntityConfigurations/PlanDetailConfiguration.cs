using Domain.Core.Entities.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class PlanDetailConfiguration : IEntityTypeConfiguration<PlanDetail>
    {
        public void Configure(EntityTypeBuilder<PlanDetail> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.PenaltyPercent).HasColumnType("decimal(6, 3)");
            builder.Property(p => p.PenaltyMaxAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.PenaltyMinAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.InterestPercent).HasColumnType("decimal(6, 3)");
            builder.Property(p => p.InterestMaxAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.InterestMinAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.WaiverPercent).HasColumnType("decimal(6, 3)");
            builder.Property(p => p.WaiverMaxAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.WaiverMinAmount).HasColumnType("decimal(32, 10)");
            builder.Property(p => p.PrepaymentPercent).HasColumnType("decimal(6, 3)");

            builder.ToTable("PlanDetail");
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}