using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerResumeConfiguration : IEntityTypeConfiguration<JobSeekerResume>
{
    public void Configure(EntityTypeBuilder<JobSeekerResume> builder)
    {
        builder.ToTable("JobSeekerResumes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobSeekerProfileId)
            .IsRequired();

        builder.Property(x => x.StoredFileId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobSeekerProfileId);
        builder.HasIndex(x => x.StoredFileId);
        builder.HasIndex(x => new { x.JobSeekerProfileId, x.IsDefault });

        builder.HasOne(x => x.JobSeekerProfile)
            .WithMany(x => x.Resumes)
            .HasForeignKey(x => x.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StoredFile)
            .WithMany()
            .HasForeignKey(x => x.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}