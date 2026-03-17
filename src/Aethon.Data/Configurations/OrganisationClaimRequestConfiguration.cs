using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationClaimRequestConfiguration : IEntityTypeConfiguration<OrganisationClaimRequest>
{
    public void Configure(EntityTypeBuilder<OrganisationClaimRequest> builder)
    {
        builder.ToTable("OrganisationClaimRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganisationId)
            .IsRequired();

        builder.Property(x => x.EmailUsed)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.EmailDomain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.VerificationToken)
            .HasMaxLength(200);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.VerificationMethod)
            .IsRequired();

        builder.HasIndex(x => new { x.OrganisationId, x.Status });
        builder.HasIndex(x => new { x.EmailDomain, x.Status });

        builder.HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}