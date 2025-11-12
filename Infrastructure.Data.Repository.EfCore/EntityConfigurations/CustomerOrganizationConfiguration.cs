using Domain.Core.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class CustomerOrganizationConfiguration : IEntityTypeConfiguration<CustomerOrganization>
    {
        public void Configure(EntityTypeBuilder<CustomerOrganization> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("CustomerOrganization");


        }
    }
}