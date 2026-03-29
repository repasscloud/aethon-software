using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerResumePerformanceConfiguration : IEntityTypeConfiguration<JobSeekerResume>
{
    public void Configure(EntityTypeBuilder<JobSeekerResume> builder)
    {
        builder.HasIndex(x => new { x.JobSeekerProfileId, x.IsActive, x.IsDefault });
        builder.HasIndex(x => new { x.JobSeekerProfileId, x.StoredFileId, x.IsActive });
    }
}
