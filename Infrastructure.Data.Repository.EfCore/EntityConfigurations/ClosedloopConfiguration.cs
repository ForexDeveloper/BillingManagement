using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class ClosedloopConfiguration : IEntityTypeConfiguration<Closedloop>
    {
        public void Configure(EntityTypeBuilder<Closedloop> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(500).IsRequired();
            builder.ToTable("Closedloop");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}