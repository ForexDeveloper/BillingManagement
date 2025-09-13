using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.MinIO.Entities;

namespace Infrastructure.Data.Repository.EfCore.EntityConfigurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.FileReference).HasMaxLength(256).IsRequired().IsUnicode(false);
        builder.Property(p => p.FileExtension).HasMaxLength(10).IsRequired().IsUnicode(false);
        builder.Property(p => p.ContentType).HasMaxLength(256).IsRequired().IsUnicode(false);
        builder.Property(p => p.OriginalFileName).HasMaxLength(400).IsUnicode();
        builder.Property(p => p.EntityId).HasMaxLength(50);

        builder.ToTable(nameof(Attachment));
    }
}