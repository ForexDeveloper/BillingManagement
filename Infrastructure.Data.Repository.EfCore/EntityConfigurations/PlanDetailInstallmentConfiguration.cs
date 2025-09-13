using Domain.Core.Entities.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class PlanDetailInstallmentConfiguration : IEntityTypeConfiguration<PlanDetailInstallment>
    {
        public void Configure(EntityTypeBuilder<PlanDetailInstallment> builder)
        {
            builder.Property(p => p.Id).IsRequired();

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("PlanDetailInstallment");
        }
    }
}