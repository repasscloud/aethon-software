using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnedByOrganisationId)
            .IsRequired();

        builder.Property(x => x.ManagedByOrganisationId);

        builder.Property(x => x.OrganisationRecruitmentPartnershipId);

        builder.Property(x => x.CreatedByIdentityUserId)
            .IsRequired();

        builder.Property(x => x.ManagedByUserId);

        builder.Property(x => x.CreatedByType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Visibility)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(150);

        builder.Property(x => x.Department)
            .HasMaxLength(150);

        builder.Property(x => x.LocationText)
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

        builder.Property(x => x.WorkplaceType)
            .IsRequired();

        builder.Property(x => x.EmploymentType)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(20000);

        builder.Property(x => x.Requirements)
            .HasMaxLength(12000);

        builder.Property(x => x.Benefits)
            .HasMaxLength(8000);

        builder.Property(x => x.Summary)
            .HasMaxLength(2000);

        builder.Property(x => x.SalaryFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryTo)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryCurrency);

        builder.Property(x => x.ExternalApplicationUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.ApplicationEmail)
            .HasMaxLength(320);

        builder.Property(x => x.CreatedForUnclaimedCompany)
            .IsRequired();

        builder.Property(x => x.PostingTier)
            .IsRequired();

        builder.Property(x => x.HighlightColour)
            .HasMaxLength(20);

        builder.Property(x => x.IsImported)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.OwnedByOrganisationId);
        builder.HasIndex(x => x.ManagedByOrganisationId);
        builder.HasIndex(x => x.OrganisationRecruitmentPartnershipId);
        builder.HasIndex(x => x.CreatedByIdentityUserId);
        builder.HasIndex(x => x.ManagedByUserId);
        builder.HasIndex(x => x.ReferenceCode);
        builder.HasIndex(x => new { x.OwnedByOrganisationId, x.Status });
        builder.HasIndex(x => new { x.OwnedByOrganisationId, x.Visibility });
        builder.HasIndex(x => x.PublishedUtc);

        builder.HasOne(x => x.OwnedByOrganisation)
            .WithMany(x => x.OwnedJobs)
            .HasForeignKey(x => x.OwnedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ManagedByOrganisation)
            .WithMany(x => x.ManagedJobs)
            .HasForeignKey(x => x.ManagedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OrganisationRecruitmentPartnership)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.OrganisationRecruitmentPartnershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByIdentityUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ManagedByUser)
            .WithMany()
            .HasForeignKey(x => x.ManagedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}