using Domain.Core.Entities.WalletContractAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class WalletContractRejectionReasonConfiguration : IEntityTypeConfiguration<WalletContractRejectionReason>
    {
        public void Configure(EntityTypeBuilder<WalletContractRejectionReason> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Reason).IsRequired(false).HasMaxLength(256);

            builder.ToTable("WalletContractRejectionReason");
            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.HasOne(x => x.WalletContract)
                .WithMany(x => x.WalletContractRejectionReasons)
                .HasForeignKey(x => x.WalletContractId);

        }
    }
}