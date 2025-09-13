using Domain.Core.Entities.FinancierAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;
public class FinancierCreditFlowConfigConfiguration : IEntityTypeConfiguration<FinancierCreditFlowConfig>
{
    public void Configure(EntityTypeBuilder<FinancierCreditFlowConfig> builder)
    {
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.ProductCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ClientId).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ClientSecret).HasMaxLength(300).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.ToTable(nameof(FinancierCreditFlowConfig));
    }
}
