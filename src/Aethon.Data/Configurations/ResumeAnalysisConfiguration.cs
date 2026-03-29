using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.ToTable("ResumeAnalyses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobSeekerResumeId).IsRequired();
        builder.Property(x => x.StoredFileId).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.HeadlineSuggestion).HasMaxLength(300);
        builder.Property(x => x.SummaryExtract).HasMaxLength(2000);
        builder.Property(x => x.SkillsJson).HasMaxLength(4000);
        builder.Property(x => x.ExperienceLevel).HasMaxLength(50);
        builder.Property(x => x.AnalysisError).HasMaxLength(2000);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobSeekerResumeId).IsUnique();
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.JobSeekerResume)
            .WithMany()
            .HasForeignKey(x => x.JobSeekerResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StoredFile)
            .WithMany()
            .HasForeignKey(x => x.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
