using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BackgroundJobAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public sealed class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
    public void Configure(EntityTypeBuilder<BackgroundJob> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.JobId).HasMaxLength(250).IsRequired();
        builder.HasIndex(p => p.JobId).IsUnique();

        builder.ToTable(nameof(BackgroundJob));
    }
}