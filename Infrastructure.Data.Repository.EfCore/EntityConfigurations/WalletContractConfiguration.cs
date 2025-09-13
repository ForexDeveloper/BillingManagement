using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class WalletContractConfiguration : IEntityTypeConfiguration<WalletContract>
    {
        public void Configure(EntityTypeBuilder<WalletContract> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Status).IsRequired();
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.StartDate).IsRequired();
            builder.Property(p => p.EndDate).IsRequired(false);

            builder.Property(e => e.ContractNumber)
            .HasMaxLength(30)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("'c-' + CAST(NEXT VALUE FOR Fc.WalletContractNumber AS NVARCHAR(30))");

            builder.HasOne(e => e.Parent)
             .WithMany()
             .HasForeignKey(e => e.ParentId)
             .IsRequired(false);

            builder.HasOne(e => e.RootParent)
            .WithMany()
            .HasForeignKey(e => e.RootParentId)
            .IsRequired(false);

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.WalletContracts)
                .HasForeignKey(x => x.OrganizationId);

            builder.HasOne(x => x.TenantIpgSetting)
                .WithMany(x => x.WalletContracts)
                .HasForeignKey(x => x.TenantIpgSettingId);

            builder.ToTable("WalletContract");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}