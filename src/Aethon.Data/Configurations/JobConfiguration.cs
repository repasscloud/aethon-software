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

        builder.Property(x => x.ManagedByOrganisationId)
            .IsRequired();

        builder.Property(x => x.CompanyRecruiterRelationshipId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(x => x.Department)
            .HasMaxLength(150);

        builder.Property(x => x.LocationText)
            .HasMaxLength(250);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(20000);

        builder.Property(x => x.Requirements)
            .HasMaxLength(20000);

        builder.Property(x => x.Benefits)
            .HasMaxLength(20000);

        builder.Property(x => x.SalaryFrom)
            .HasPrecision(18, 2);

        builder.Property(x => x.SalaryTo)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Visibility)
            .IsRequired();

        builder.Property(x => x.CreatedByType)
            .IsRequired();

        builder.Property(x => x.WorkplaceType)
            .IsRequired();

        builder.Property(x => x.EmploymentType)
            .IsRequired();

        builder.Property(x => x.CreatedForUnclaimedCompany)
            .IsRequired();

        builder.Property(x => x.CreatedByIdentityUserId)
            .IsRequired();

        builder.Property(x => x.ApprovedByUserId);

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => new { x.OwnedByOrganisationId, x.Status });
        builder.HasIndex(x => new { x.ManagedByOrganisationId, x.Status });
        builder.HasIndex(x => new { x.Visibility, x.Status });
        builder.HasIndex(x => x.ReferenceCode);

        builder.HasOne(x => x.OwnedByOrganisation)
            .WithMany(x => x.OwnedJobs)
            .HasForeignKey(x => x.OwnedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ManagedByOrganisation)
            .WithMany(x => x.ManagedJobs)
            .HasForeignKey(x => x.ManagedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CompanyRecruiterRelationship)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.CompanyRecruiterRelationshipId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByIdentityUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Applications)
            .WithOne(x => x.Job)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}