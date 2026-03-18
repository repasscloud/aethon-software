using Aethon.Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.UserType)
            .IsRequired();

        builder.Property(x => x.IsIdentityVerified)
            .IsRequired();

        builder.Property(x => x.IdentityVerificationNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.IsPhoneNumberVerified)
            .IsRequired();

        builder.HasIndex(x => x.UserType);
        builder.HasIndex(x => x.IsEnabled);
        builder.HasIndex(x => x.IsIdentityVerified);
        builder.HasIndex(x => x.IsPhoneNumberVerified);
    }
}