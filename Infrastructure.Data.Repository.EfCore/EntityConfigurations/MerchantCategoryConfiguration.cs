using Domain.Core.Entities.Merchants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class MerchantCategoryConfiguration : IEntityTypeConfiguration<MerchantCategory>
{
    public void Configure(EntityTypeBuilder<MerchantCategory> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(c => c.Merchant)
        .WithMany(c => c.MerchantCategories)
        .HasForeignKey(c => c.MerchantId);

        builder.HasOne(c => c.Category)
       .WithMany(c => c.MerchantAndCategories)
       .HasForeignKey(c => c.CategoryId);

        builder.ToTable("MerchantCategory");
    }
}
