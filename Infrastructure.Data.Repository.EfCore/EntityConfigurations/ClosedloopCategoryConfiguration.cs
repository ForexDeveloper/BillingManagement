using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class ClosedloopCategoryConfiguration : IEntityTypeConfiguration<ClosedloopCategory>
    {
        public void Configure(EntityTypeBuilder<ClosedloopCategory> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.ToTable("ClosedloopCategory");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}