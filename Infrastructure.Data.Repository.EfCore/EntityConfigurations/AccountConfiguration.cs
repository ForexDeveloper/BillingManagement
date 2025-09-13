using Domain.Core.Entities.AccountAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.Property(p => p.Id).IsRequired();

            builder.Property(p => p.Balance).HasColumnType("decimal(32, 10)").IsRequired();
            builder.Property(p => p.NonWithDrawableBalance).HasColumnType("decimal(32, 10)").IsRequired();


            builder.Property(p => p.CheckSum).HasMaxLength(500).IsRequired();

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.ToTable("Account");
        }
    }
}