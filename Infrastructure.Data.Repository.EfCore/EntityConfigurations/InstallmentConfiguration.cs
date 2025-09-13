using Domain.Core.Entities.InstallmentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
    {
        public void Configure(EntityTypeBuilder<Installment> builder)
        {
            builder.Property(p => p.Id).IsRequired();

            builder.Property(p => p.Amount).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.PaidAmount).HasColumnType("decimal(32, 10)").IsRequired();

            builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();
            builder.Property(p => p.SayyadIdentifier).HasMaxLength(16);
            builder.Property(p => p.HasBilling).IsRequired();


            builder.Property(e => e.RowVersion)
                   .IsRowVersion();

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("Installment");
        }
    }
}