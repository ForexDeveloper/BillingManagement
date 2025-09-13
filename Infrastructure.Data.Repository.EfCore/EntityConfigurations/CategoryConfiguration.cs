using Domain.Core.AggregateRoots.CategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();


            builder.HasOne(c => c.Parent)
               .WithMany(x => x.Categories)
               .HasForeignKey(c => c.ParentId)
               .IsRequired(false);
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("Category");
        }
    }
}