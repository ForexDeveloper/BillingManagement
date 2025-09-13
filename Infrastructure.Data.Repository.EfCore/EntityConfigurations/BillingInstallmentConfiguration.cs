using Domain.Core.Entities.BillingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations;

public class BillingInstallmentConfiguration : IEntityTypeConfiguration<BillingInstallment>
{
    public void Configure(EntityTypeBuilder<BillingInstallment> builder)
    {
        builder.Property(p => p.Id).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable("BillingInstallment");
    }
}