using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class IdentityVerificationRequestConfiguration : IEntityTypeConfiguration<IdentityVerificationRequest>
{
    public void Configure(EntityTypeBuilder<IdentityVerificationRequest> builder)
    {
        builder.ToTable("IdentityVerificationRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.EmailAddress)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AdditionalNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ReviewNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        // One pending request per user at a time
        builder.HasIndex(x => new { x.UserId, x.Status })
            .IsUnique()
            .HasFilter("[Status] = 1"); // 1 = Pending

        // Fast lookup for admin queue
        builder.HasIndex(x => x.Status);

        // Fast lookup by user
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
