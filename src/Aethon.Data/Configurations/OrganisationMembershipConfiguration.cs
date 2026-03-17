using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationMembershipConfiguration : IEntityTypeConfiguration<OrganisationMembership>
{
    public void Configure(EntityTypeBuilder<OrganisationMembership> builder)
    {
        builder.ToTable("OrganisationMemberships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganisationId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => new { x.OrganisationId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => new { x.OrganisationId, x.IsOwner });

        builder.HasOne(x => x.Organisation)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}