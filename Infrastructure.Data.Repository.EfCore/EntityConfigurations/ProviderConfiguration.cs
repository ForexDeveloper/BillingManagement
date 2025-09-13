using Domain.Core.Entities.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.Property(p => p.ProviderType).IsRequired();
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2500);
        builder.Property(p => p.Name).HasMaxLength(250);
        builder.Property(p => p.EnglishName).IsRequired().HasMaxLength(250);

        builder.ToTable(nameof(Provider));
    }
}