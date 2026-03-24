using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class JobSeekerCertificateConfiguration : IEntityTypeConfiguration<JobSeekerCertificate>
{
    public void Configure(EntityTypeBuilder<JobSeekerCertificate> builder)
    {
        builder.ToTable("JobSeekerCertificates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IssuingOrganisation).HasMaxLength(200);
        builder.Property(x => x.CredentialId).HasMaxLength(200);
        builder.Property(x => x.CredentialUrl).HasMaxLength(1000);

        builder.HasIndex(x => x.JobSeekerProfileId);

        builder.HasOne(x => x.Profile)
            .WithMany(p => p.Certificates)
            .HasForeignKey(x => x.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
