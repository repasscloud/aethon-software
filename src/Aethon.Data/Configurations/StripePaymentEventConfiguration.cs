using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class StripePaymentEventConfiguration : IEntityTypeConfiguration<StripePaymentEvent>
{
    public void Configure(EntityTypeBuilder<StripePaymentEvent> builder)
    {
        builder.ToTable("StripePaymentEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripeEventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.StripeEventId)
            .IsUnique();

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(255);

        builder.Property(x => x.PayloadJson)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();
    }
}
