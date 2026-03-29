using Aethon.Api.Common;
using Aethon.Application.Integrations.Commands.CreateWebhookSubscription;
using Aethon.Application.Integrations.Queries.GetWebhookSubscriptions;

namespace Aethon.Api.Endpoints.Integrations;

public static class IntegrationEndpoints
{
    public static void MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/integrations")
            .RequireAuthorization()
            .WithTags("Integrations");

        group.MapGet("/organisations/{organisationId:guid}/webhooks", async (
            GetWebhookSubscriptionsHandler handler,
            Guid organisationId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetWebhookSubscriptionsQuery
                {
                    OrganisationId = organisationId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapPost("/organisations/{organisationId:guid}/webhooks", async (
            CreateWebhookSubscriptionHandler handler,
            HttpContext httpContext,
            Guid organisationId,
            CreateWebhookSubscriptionCommand request,
            CancellationToken ct) =>
        {
            var command = new CreateWebhookSubscriptionCommand
            {
                OrganisationId = organisationId,
                Name = request.Name,
                EndpointUrl = request.EndpointUrl,
                Secret = request.Secret,
                Events = request.Events
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });
    }
}
