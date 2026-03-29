using Microsoft.EntityFrameworkCore;
using Aethon.Data.Entities;

namespace Aethon.Application.Abstractions.Persistence;

public interface IAethonDbContext
{
    DbSet<Job> Jobs { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<Organisation> Organisations { get; }
    DbSet<OrganisationMembership> OrganisationMemberships { get; }
    DbSet<JobSeekerProfile> JobSeekerProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}