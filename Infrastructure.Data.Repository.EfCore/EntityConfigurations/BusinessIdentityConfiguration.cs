using Domain.Core.Entities.BusinessEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class BusinessIdentityConfiguration : IEntityTypeConfiguration<BusinessIdentity>
    {
        public void Configure(EntityTypeBuilder<BusinessIdentity> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Id).IsRequired();
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.UseTptMappingStrategy();

            builder.ToTable("BusinessIdentity");
        }
    }

}
