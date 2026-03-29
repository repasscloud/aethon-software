using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationStatusHistoryPerformanceConfiguration : IEntityTypeConfiguration<JobApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<JobApplicationStatusHistory> builder)
    {
        builder.HasIndex(x => new { x.JobApplicationId, x.ChangedUtc });
    }
}
