using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerQualificationConfiguration : IEntityTypeConfiguration<JobSeekerQualification>
{
    public void Configure(EntityTypeBuilder<JobSeekerQualification> builder)
    {
        builder.ToTable("JobSeekerQualifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Institution).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => x.JobSeekerProfileId);

        builder.HasOne(x => x.Profile)
            .WithMany(p => p.Qualifications)
            .HasForeignKey(x => x.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
