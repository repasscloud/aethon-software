using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationPerformanceConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.SubmittedUtc });
        builder.HasIndex(x => new { x.JobId, x.SubmittedUtc });
        builder.HasIndex(x => new { x.JobId, x.Status, x.SubmittedUtc });
        builder.HasIndex(x => new { x.UserId, x.Status, x.SubmittedUtc });
    }
}
