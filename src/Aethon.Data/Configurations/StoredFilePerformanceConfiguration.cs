using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class StoredFilePerformanceConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasIndex(x => new { x.UploadedByUserId, x.CreatedUtc });
        builder.HasIndex(x => x.StoragePath)
            .IsUnique();
    }
}
