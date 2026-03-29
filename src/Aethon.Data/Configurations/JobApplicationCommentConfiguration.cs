using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobApplicationCommentConfiguration : IEntityTypeConfiguration<JobApplicationComment>
{
    public void Configure(EntityTypeBuilder<JobApplicationComment> builder)
    {
        builder.ToTable("JobApplicationComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobApplicationId)
            .IsRequired();

        builder.Property(x => x.ParentCommentId);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedByUserId);

        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);

        builder.HasIndex(x => x.JobApplicationId);
        builder.HasIndex(x => x.ParentCommentId);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.UpdatedByUserId);
        builder.HasIndex(x => x.DeletedByUserId);

        builder.HasOne(x => x.JobApplication)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeletedByUser)
            .WithMany()
            .HasForeignKey(x => x.DeletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}