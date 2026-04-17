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

        builder.Property(x => x.RequestedByUserId)
            .IsRequired();

        builder.Property(x => x.EmailUsed)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.EmailDomain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.VerificationMethod)
            .IsRequired();

        builder.Property(x => x.VerificationToken)
            .HasMaxLength(200);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.OrganisationId);
        builder.HasIndex(x => x.RequestedByUserId);
        builder.HasIndex(x => x.EmailDomain);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.VerificationToken);

        builder.HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}