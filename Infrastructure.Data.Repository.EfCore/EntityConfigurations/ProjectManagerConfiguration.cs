using Domain.Core.Entities.ProjectManegerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneticsBank.Entities.Models.EntityConfigurations
{
    public class ProjectManagerConfiguration : IEntityTypeConfiguration<ProjectManager>
    {
        public void Configure(EntityTypeBuilder<ProjectManager> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.Id).HasAnnotation("SqlServer:Identity", "1, 1");

            builder.Property(p => p.FullName).HasMaxLength(500).IsRequired();
            builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();
            builder.ToTable("ProjectManager");
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}