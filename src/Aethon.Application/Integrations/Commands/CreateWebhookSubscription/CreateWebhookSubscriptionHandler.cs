using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Integrations;

namespace Aethon.Application.Integrations.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly OrganisationAccessService _organisationAccessService;

    public CreateWebhookSubscriptionHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        OrganisationAccessService organisationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _organisationAccessService = organisationAccessService;
    }

    public async Task<Result<WebhookSubscriptionDto>> HandleAsync(
        CreateWebhookSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<WebhookSubscriptionDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var canCreate = await _organisationAccessService.CanCreateJobsAsync(
            _currentUserAccessor.UserId,
            command.OrganisationId,
            cancellationToken);

        if (!canCreate)
        {
            return Result<WebhookSubscriptionDto>.Failure(
                "integrations.forbidden",
                "The current user cannot manage integrations for this organisation.");
        }

        var normalizedEvents = command.Events
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var entity = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            OrganisationId = command.OrganisationId,
            Name = command.Name.Trim(),
            EndpointUrl = command.EndpointUrl.Trim(),
            Secret = command.Secret.Trim(),
            EventsCsv = string.Join(',', normalizedEvents),
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUserAccessor.UserId
        };

        _dbContext.WebhookSubscriptions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<WebhookSubscriptionDto>.Success(new WebhookSubscriptionDto
        {
            Id = entity.Id,
            OrganisationId = entity.OrganisationId,
            Name = entity.Name,
            EndpointUrl = entity.EndpointUrl,
            Secret = entity.Secret,
            IsActive = entity.IsActive,
            Events = normalizedEvents
        });
    }
}
