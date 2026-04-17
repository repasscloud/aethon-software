using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSyndicationRecordConfiguration : IEntityTypeConfiguration<JobSyndicationRecord>
{
    public void Configure(EntityTypeBuilder<JobSyndicationRecord> builder)
    {
        builder.ToTable("JobSyndicationRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ExternalRef)
            .HasMaxLength(500);

        builder.Property(x => x.SubmittedUtc)
            .IsRequired();

        builder.Property(x => x.LastAttemptUtc);

        builder.Property(x => x.LastErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.JobId, x.Provider });
        builder.HasIndex(x => x.SubmittedUtc);

        builder.HasOne(x => x.Job)
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
