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

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(1000);

        builder.Property(x => x.ResumeFileId);

        builder.Property(x => x.CoverLetter)
            .HasMaxLength(20000);

        builder.Property(x => x.AssignedRecruiterUserId);

        builder.Property(x => x.AssignedToUserId);

        builder.Property(x => x.SubmittedUtc)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(100);

        builder.Property(x => x.SourceDetail)
            .HasMaxLength(250);

        builder.Property(x => x.SourceReference)
            .HasMaxLength(150);

        builder.Property(x => x.InternalSummaryNotes)
            .HasMaxLength(4000);

        builder.Property(x => x.ScreeningSummary)
            .HasMaxLength(4000);

        builder.Property(x => x.Rating)
            .HasPrecision(5, 2);

        builder.Property(x => x.Recommendation)
            .HasMaxLength(100);

        builder.Property(x => x.Tags)
            .HasMaxLength(1000);

        builder.Property(x => x.CandidatePhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.CandidateLocationText)
            .HasMaxLength(250);

        builder.Property(x => x.AvailabilityText)
            .HasMaxLength(250);

        builder.Property(x => x.SalaryExpectation)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryExpectationCurrency);

        builder.Property(x => x.AcceptedPrivacyPolicy)
            .IsRequired();

        builder.Property(x => x.WithdrawalReason)
            .HasMaxLength(1000);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(150);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => new { x.JobId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => new { x.JobId, x.Status });
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.AssignedRecruiterUserId);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => x.ResumeFileId);
        builder.HasIndex(x => x.SubmittedUtc);
        builder.HasIndex(x => x.LastStatusChangedUtc);
        builder.HasIndex(x => x.LastActivityUtc);
        builder.HasIndex(x => x.IsWithdrawn);
        builder.HasIndex(x => x.IsRejected);
        builder.HasIndex(x => x.IsHired);
        builder.HasIndex(x => x.IsArchived);

        builder.HasOne(x => x.Job)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResumeFile)
            .WithMany()
            .HasForeignKey(x => x.ResumeFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssignedRecruiterUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedRecruiterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WithdrawnByUser)
            .WithMany()
            .HasForeignKey(x => x.WithdrawnByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RejectedByUser)
            .WithMany()
            .HasForeignKey(x => x.RejectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DuplicateOfApplication)
            .WithMany()
            .HasForeignKey(x => x.DuplicateOfApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}