using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class WalletContractPlanConfiguration : IEntityTypeConfiguration<WalletContractPlan>
{
    public void Configure(EntityTypeBuilder<WalletContractPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();

        builder.ToTable("WalletContractPlan");
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(x => x.WalletContract)
            .WithMany(x => x.WalletContractPlans)
            .HasForeignKey(x => x.WalletContractId);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.WalletContractPlans)
            .HasForeignKey(x => x.PlanId);
    }
}