using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationInterviewInterviewerConfiguration : IEntityTypeConfiguration<JobApplicationInterviewInterviewer>
{
    public void Configure(EntityTypeBuilder<JobApplicationInterviewInterviewer> builder)
    {
        builder.ToTable("JobApplicationInterviewInterviewers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobApplicationInterviewId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RoleLabel)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => new { x.JobApplicationInterviewId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.JobApplicationInterview)
            .WithMany(x => x.Interviewers)
            .HasForeignKey(x => x.JobApplicationInterviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}