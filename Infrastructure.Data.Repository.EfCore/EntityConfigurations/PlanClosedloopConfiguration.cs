using Domain.Core.Entities.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class PlanClosedloopConfiguration : IEntityTypeConfiguration<PlanClosedloop>
    {
        public void Configure(EntityTypeBuilder<PlanClosedloop> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.ToTable("PlanClosedloop");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}