using Domain.Core.Entities.OrganizationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(250).IsRequired();

            builder
                .HasOne(o => o.Parent)
                .WithMany()
                .HasForeignKey(o => o.ParentId);

            builder.ToTable("Organization");


        }
    }
}