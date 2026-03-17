using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId)
            .IsRequired();

        builder.Property(x => x.ResumeFileId);

        builder.Property(x => x.CoverLetter)
            .HasMaxLength(20000);

        builder.Property(x => x.Source)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => new { x.JobId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => new { x.JobId, x.Status });

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}