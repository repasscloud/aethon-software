using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationDomainConfiguration : IEntityTypeConfiguration<OrganisationDomain>
{
    public void Configure(EntityTypeBuilder<OrganisationDomain> builder)
    {
        builder.ToTable("OrganisationDomains");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganisationId)
            .IsRequired();

        builder.Property(x => x.Domain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NormalizedDomain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.VerificationMethod)
            .IsRequired();

        builder.Property(x => x.TrustLevel)
            .IsRequired();

        builder.Property(x => x.VerificationToken)
            .HasMaxLength(200);

        builder.Property(x => x.VerificationDnsRecordName)
            .HasMaxLength(255);

        builder.Property(x => x.VerificationDnsRecordValue)
            .HasMaxLength(500);

        builder.Property(x => x.VerificationEmailAddress)
            .HasMaxLength(320);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.NormalizedDomain)
            .IsUnique();

        builder.HasIndex(x => x.OrganisationId);
        builder.HasIndex(x => new { x.OrganisationId, x.IsPrimary });
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Organisation)
            .WithMany(x => x.Domains)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}