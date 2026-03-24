using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerWorkExperienceConfiguration : IEntityTypeConfiguration<JobSeekerWorkExperience>
{
    public void Configure(EntityTypeBuilder<JobSeekerWorkExperience> builder)
    {
        builder.ToTable("JobSeekerWorkExperiences");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobTitle).IsRequired().HasMaxLength(200);
        builder.Property(x => x.EmployerName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(4000);

        builder.HasIndex(x => x.JobSeekerProfileId);

        builder.HasOne(x => x.Profile)
            .WithMany(p => p.WorkExperiences)
            .HasForeignKey(x => x.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
