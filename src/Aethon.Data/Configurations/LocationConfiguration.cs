using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.City).HasMaxLength(150);
        builder.Property(x => x.State).HasMaxLength(150);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.CountryCode).HasMaxLength(10);

        builder.HasIndex(x => x.DisplayName);
        builder.HasIndex(x => new { x.IsActive, x.SortOrder });
    }
}
