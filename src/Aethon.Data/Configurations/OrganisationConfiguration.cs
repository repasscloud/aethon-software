using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("Organisations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ClaimStatus)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.LegalName)
            .HasMaxLength(250);

        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Slug)
            .HasMaxLength(150);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.Summary)
            .HasMaxLength(4000);

        builder.Property(x => x.PublicLocationText)
            .HasMaxLength(250);

        builder.Property(x => x.LocationCity)
            .HasMaxLength(150);

        builder.Property(x => x.LocationState)
            .HasMaxLength(150);

        builder.Property(x => x.LocationCountry)
            .HasMaxLength(100);

        builder.Property(x => x.LocationCountryCode)
            .HasMaxLength(10);

        builder.Property(x => x.LocationLatitude);

        builder.Property(x => x.LocationLongitude);

        builder.Property(x => x.LocationPlaceId)
            .HasMaxLength(500);

        builder.Property(x => x.PublicContactEmail)
            .HasMaxLength(320);

        builder.Property(x => x.PublicContactPhone)
            .HasMaxLength(50);

        builder.Property(x => x.IsPublicProfileEnabled)
            .IsRequired();

        builder.Property(x => x.PrimaryContactName)
            .HasMaxLength(150);

        builder.Property(x => x.PrimaryContactEmail)
            .HasMaxLength(320);

        builder.Property(x => x.PrimaryContactPhone)
            .HasMaxLength(50);

        builder.Ignore(x => x.IsVerified);

        builder.Property(x => x.VerificationTier)
            .IsRequired()
            .HasDefaultValue(Aethon.Shared.Enums.VerificationTier.None);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.NormalizedName);
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"Slug\" IS NOT NULL");
        builder.HasIndex(x => x.PrimaryDomainId);
        builder.HasIndex(x => new { x.Type, x.Status });
        builder.HasIndex(x => x.VerificationTier);

        builder.HasOne(x => x.PrimaryDomain)
            .WithMany()
            .HasForeignKey(x => x.PrimaryDomainId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}