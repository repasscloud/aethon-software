using Aethon.Data;
using Aethon.Data.Entities;

namespace Aethon.Application.Activity.Services;

public sealed class ActivityLogWriter
{
    private readonly AethonDbContext _dbContext;

    public ActivityLogWriter(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task WriteAsync(
        string entityType,
        Guid entityId,
        string action,
        DateTime performedUtc,
        Guid? performedByUserId = null,
        Guid? organisationId = null,
        string? summary = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var activity = new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Summary = summary,
            Details = details,
            OrganisationId = organisationId,
            PerformedByUserId = performedByUserId,
            PerformedUtc = performedUtc,
            CreatedUtc = performedUtc,
            CreatedByUserId = performedByUserId
        };

        _dbContext.ActivityLogs.Add(activity);

        return Task.CompletedTask;
    }
}