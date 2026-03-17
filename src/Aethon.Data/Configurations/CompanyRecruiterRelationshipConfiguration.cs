using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class CompanyRecruiterRelationshipConfiguration : IEntityTypeConfiguration<CompanyRecruiterRelationship>
{
    public void Configure(EntityTypeBuilder<CompanyRecruiterRelationship> builder)
    {
        builder.ToTable("CompanyRecruiterRelationships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyOrganisationId)
            .IsRequired();

        builder.Property(x => x.RecruiterOrganisationId)
            .IsRequired();

        builder.Property(x => x.RequestedByUserId);

        builder.Property(x => x.ApprovedByUserId);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Scope)
            .IsRequired();

        builder.Property(x => x.RecruiterCanCreateUnclaimedCompanyJobs)
            .IsRequired();

        builder.Property(x => x.RecruiterCanPublishJobs)
            .IsRequired();

        builder.Property(x => x.RecruiterCanManageCandidates)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => new { x.CompanyOrganisationId, x.RecruiterOrganisationId })
            .IsUnique();

        builder.HasOne(x => x.CompanyOrganisation)
            .WithMany(x => x.CompanyRelationships)
            .HasForeignKey(x => x.CompanyOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecruiterOrganisation)
            .WithMany(x => x.RecruiterRelationships)
            .HasForeignKey(x => x.RecruiterOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Jobs)
            .WithOne(x => x.CompanyRecruiterRelationship)
            .HasForeignKey(x => x.CompanyRecruiterRelationshipId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}