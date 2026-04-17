using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Integrations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Integrations.Queries.GetWebhookSubscriptions;

public sealed class GetWebhookSubscriptionsHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly OrganisationAccessService _organisationAccessService;

    public GetWebhookSubscriptionsHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        OrganisationAccessService organisationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _organisationAccessService = organisationAccessService;
    }

    public async Task<Result<IReadOnlyList<WebhookSubscriptionDto>>> HandleAsync(
        GetWebhookSubscriptionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<IReadOnlyList<WebhookSubscriptionDto>>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var canView = await _organisationAccessService.IsActiveMemberAsync(
            _currentUserAccessor.UserId,
            query.OrganisationId,
            cancellationToken);

        if (!canView)
        {
            return Result<IReadOnlyList<WebhookSubscriptionDto>>.Failure(
                "integrations.forbidden",
                "The current user cannot view integrations for this organisation.");
        }

        var items = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(x => x.OrganisationId == query.OrganisationId)
            .OrderBy(x => x.Name)
            .Select(x => new WebhookSubscriptionDto
            {
                Id = x.Id,
                OrganisationId = x.OrganisationId,
                Name = x.Name,
                EndpointUrl = x.EndpointUrl,
                Secret = x.Secret,
                IsActive = x.IsActive,
                Events = x.EventsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<WebhookSubscriptionDto>>.Success(items);
    }
}
