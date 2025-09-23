using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.EventBus.Entities;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations
{
    public class OutboxEntityConfiguration : IEntityTypeConfiguration<OutboxEntity>
    {
        public void Configure(EntityTypeBuilder<OutboxEntity> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired();
            builder.Property(p => p.Message).IsRequired();
            builder.Property(p => p.EventName).HasMaxLength(1000).IsRequired().IsUnicode(false);

            builder.ToTable("Outbox");
        }
    }
}