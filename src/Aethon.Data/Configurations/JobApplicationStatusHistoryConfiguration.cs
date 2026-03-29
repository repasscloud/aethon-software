using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<JobApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<JobApplicationStatusHistory> builder)
    {
        builder.ToTable("JobApplicationStatusHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobApplicationId)
            .IsRequired();

        builder.Property(x => x.FromStatus);

        builder.Property(x => x.ToStatus)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.ChangedByUserId)
            .IsRequired();

        builder.Property(x => x.ChangedUtc)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobApplicationId);
        builder.HasIndex(x => x.ChangedByUserId);
        builder.HasIndex(x => x.ChangedUtc);

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}