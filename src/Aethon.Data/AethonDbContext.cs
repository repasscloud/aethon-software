using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Data;

public sealed class AethonDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AethonDbContext(DbContextOptions<AethonDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenantMembership> UserTenantMemberships => Set<UserTenantMembership>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<UserTenantMembership>(entity =>
        {
            entity.ToTable("user_tenant_memberships");
            entity.HasKey(x => new { x.UserId, x.TenantId });

            entity.Property(x => x.RoleCode).HasMaxLength(100).IsRequired();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
