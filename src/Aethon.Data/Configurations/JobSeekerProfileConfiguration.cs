using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerProfileConfiguration : IEntityTypeConfiguration<JobSeekerProfile>
{
    public void Configure(EntityTypeBuilder<JobSeekerProfile> builder)
    {
        builder.ToTable("JobSeekerProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.MiddleName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(x => x.WhatsAppNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Headline)
            .HasMaxLength(250);

        builder.Property(x => x.Summary)
            .HasMaxLength(4000);

        builder.Property(x => x.CurrentLocation)
            .HasMaxLength(250);

        builder.Property(x => x.PreferredLocation)
            .HasMaxLength(250);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.OpenToWork)
            .IsRequired();

        builder.Property(x => x.DesiredSalaryFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.DesiredSalaryTo)
            .HasPrecision(18, 2);

        builder.Property(x => x.DesiredSalaryCurrency);

        builder.Property(x => x.AvailabilityText)
            .HasMaxLength(250);

        builder.Property(x => x.IsPublicProfileEnabled)
            .IsRequired();

        builder.Property(x => x.IsSearchable)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(150);

        builder.Property(x => x.AboutMe)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasIndex(x => x.OpenToWork);
        builder.HasIndex(x => x.IsPublicProfileEnabled);
        builder.HasIndex(x => x.IsSearchable);
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("[Slug] IS NOT NULL");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}