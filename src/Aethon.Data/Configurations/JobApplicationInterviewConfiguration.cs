using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationInterviewConfiguration : IEntityTypeConfiguration<JobApplicationInterview>
{
    public void Configure(EntityTypeBuilder<JobApplicationInterview> builder)
    {
        builder.ToTable("JobApplicationInterviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobApplicationId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250);

        builder.Property(x => x.Location)
            .HasMaxLength(500);

        builder.Property(x => x.MeetingUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.ScheduledStartUtc)
            .IsRequired();

        builder.Property(x => x.ScheduledEndUtc)
            .IsRequired();

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobApplicationId);
        builder.HasIndex(x => new { x.JobApplicationId, x.Status });
        builder.HasIndex(x => x.ScheduledStartUtc);

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}