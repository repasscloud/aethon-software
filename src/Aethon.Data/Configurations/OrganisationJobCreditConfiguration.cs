using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aethon.Data.Configurations;

public sealed class OrganisationJobCreditConfiguration : IEntityTypeConfiguration<OrganisationJobCredit>
{
    public void Configure(EntityTypeBuilder<OrganisationJobCredit> builder)
    {
        builder.ToTable("OrganisationJobCredits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreditType).IsRequired();
        builder.Property(x => x.Source).IsRequired();
        builder.Property(x => x.QuantityOriginal).IsRequired();
        builder.Property(x => x.QuantityRemaining).IsRequired();

        builder.Property(x => x.GrantNote).HasMaxLength(500);

        builder.HasIndex(x => x.OrganisationId);
        builder.HasIndex(x => new { x.OrganisationId, x.CreditType, x.QuantityRemaining });

        builder.HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StripePaymentEvent)
            .WithMany()
            .HasForeignKey(x => x.StripePaymentEventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
