using Domain.Core.Entities.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class MerchantBranchConfiguration : IEntityTypeConfiguration<MerchantBranch>
    {
        public void Configure(EntityTypeBuilder<MerchantBranch> builder)
        {
    
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();

            builder.ToTable("MerchantBranch");
        }
    }
}