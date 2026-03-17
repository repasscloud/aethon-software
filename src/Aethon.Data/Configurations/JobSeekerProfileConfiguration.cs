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

        builder.Property(x => x.Headline)
            .HasMaxLength(250);

        builder.Property(x => x.Summary)
            .HasMaxLength(4000);

        builder.Property(x => x.CurrentLocation)
            .HasMaxLength(250);

        builder.Property(x => x.PreferredLocation)
            .HasMaxLength(250);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DesiredSalaryFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.DesiredSalaryTo)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<JobSeekerProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}