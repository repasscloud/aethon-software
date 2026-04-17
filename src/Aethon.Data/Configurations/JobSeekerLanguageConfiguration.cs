using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerLanguageConfiguration : IEntityTypeConfiguration<JobSeekerLanguage>
{
    public void Configure(EntityTypeBuilder<JobSeekerLanguage> builder)
    {
        builder.ToTable("JobSeekerLanguages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobSeekerProfileId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AbilityType)
            .IsRequired();

        builder.Property(x => x.ProficiencyLevel);

        builder.Property(x => x.IsVerified)
            .IsRequired();

        builder.Property(x => x.VerificationNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobSeekerProfileId);
        builder.HasIndex(x => new { x.JobSeekerProfileId, x.Name, x.AbilityType });

        builder.HasOne(x => x.JobSeekerProfile)
            .WithMany(x => x.Languages)
            .HasForeignKey(x => x.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}