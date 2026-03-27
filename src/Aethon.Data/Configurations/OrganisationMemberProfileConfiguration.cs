using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationMemberProfileConfiguration : IEntityTypeConfiguration<OrganisationMemberProfile>
{
    public void Configure(EntityTypeBuilder<OrganisationMemberProfile> builder)
    {
        builder.ToTable("OrganisationMemberProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.OrganisationId)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(60);

        builder.Property(x => x.JobTitle)
            .HasMaxLength(200);

        builder.Property(x => x.Bio)
            .HasMaxLength(2000);

        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.PublicEmail)
            .HasMaxLength(256);

        builder.Property(x => x.PublicPhone)
            .HasMaxLength(50);

        builder.Property(x => x.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsPublicProfileEnabled)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        // One profile per user per organisation
        builder.HasIndex(x => new { x.OrganisationId, x.UserId })
            .IsUnique();

        // Slug unique within an org (filtered — null slugs don't conflict)
        builder.HasIndex(x => new { x.OrganisationId, x.Slug })
            .IsUnique()
            .HasFilter("[Slug] IS NOT NULL");

        // Fast lookup for public team page
        builder.HasIndex(x => new { x.OrganisationId, x.IsPublicProfileEnabled });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
