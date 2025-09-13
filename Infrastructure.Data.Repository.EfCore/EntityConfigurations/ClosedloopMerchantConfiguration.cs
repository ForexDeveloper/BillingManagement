using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class ClosedloopMerchantConfiguration : IEntityTypeConfiguration<ClosedloopMerchant>
    {
        public void Configure(EntityTypeBuilder<ClosedloopMerchant> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.ToTable("ClosedloopMerchant");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}