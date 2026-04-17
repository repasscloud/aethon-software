using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class CreditConsumptionLogConfiguration : IEntityTypeConfiguration<CreditConsumptionLog>
{
    public void Configure(EntityTypeBuilder<CreditConsumptionLog> builder)
    {
        builder.ToTable("CreditConsumptionLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityConsumed).IsRequired();
        builder.Property(x => x.ConsumedAt).IsRequired();

        builder.HasIndex(x => x.OrganisationJobCreditId);
        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => x.OrganisationId);

        builder.HasOne(x => x.Credit)
            .WithMany(x => x.ConsumptionLogs)
            .HasForeignKey(x => x.OrganisationJobCreditId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Job)
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
