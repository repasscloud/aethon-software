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

        builder.Property(x => x.Id)
            .HasMaxLength(64);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.LegalName)
            .HasMaxLength(250);

        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PrimaryDomainId)
            .HasMaxLength(64);

        builder.Property(x => x.ClaimedByUserId)
            .HasMaxLength(64);

        builder.Property(x => x.PrimaryContactName)
            .HasMaxLength(250);

        builder.Property(x => x.PrimaryContactEmail)
            .HasMaxLength(320);

        builder.Property(x => x.PrimaryContactPhone)
            .HasMaxLength(50);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ClaimStatus)
            .IsRequired();

        builder.Property(x => x.IsProvisionedByRecruiter)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(64);

        builder.Property(x => x.UpdatedByUserId)
            .HasMaxLength(64);

        builder.HasIndex(x => new { x.Type, x.NormalizedName });

        builder.HasOne(x => x.PrimaryDomain)
            .WithMany()
            .HasForeignKey(x => x.PrimaryDomainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Domains)
            .WithOne(x => x.Organisation)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Memberships)
            .WithOne(x => x.Organisation)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Invitations)
            .WithOne(x => x.Organisation)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CompanyRelationships)
            .WithOne(x => x.CompanyOrganisation)
            .HasForeignKey(x => x.CompanyOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RecruiterRelationships)
            .WithOne(x => x.RecruiterOrganisation)
            .HasForeignKey(x => x.RecruiterOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OwnedJobs)
            .WithOne(x => x.OwnedByOrganisation)
            .HasForeignKey(x => x.OwnedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ManagedJobs)
            .WithOne(x => x.ManagedByOrganisation)
            .HasForeignKey(x => x.ManagedByOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}