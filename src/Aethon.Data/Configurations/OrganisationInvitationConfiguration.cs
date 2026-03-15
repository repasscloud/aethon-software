using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationInvitationConfiguration : IEntityTypeConfiguration<OrganisationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganisationInvitation> builder)
    {
        builder.ToTable("OrganisationInvitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(64);

        builder.Property(x => x.OrganisationId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.EmailDomain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AcceptedByUserId)
            .HasMaxLength(64);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(64);

        builder.Property(x => x.UpdatedByUserId)
            .HasMaxLength(64);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => new { x.OrganisationId, x.NormalizedEmail, x.Type, x.Status });

        builder.HasOne(x => x.Organisation)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}