using Aethon.Api.Endpoints.Admin;
using Aethon.Api.Endpoints.Applications;
using Aethon.Api.Endpoints.Auth;
using Aethon.Api.Endpoints.Billing;
using Aethon.Api.Endpoints.Candidates;
using Aethon.Api.Endpoints.CompanyJobs;
using Aethon.Api.Endpoints.CompanyRecruiters;
using Aethon.Api.Endpoints.Files;
using Aethon.Api.Endpoints.Identity;
using Aethon.Api.Endpoints.Import;
using Aethon.Api.Endpoints.Integrations;
using Aethon.Api.Endpoints.Jobs;
using Aethon.Api.Endpoints.Organisations;
using Aethon.Api.Endpoints.Public;
using Aethon.Api.Endpoints.RecruiterCompanies;
using Aethon.Api.Endpoints.RecruiterJobs;
using Aethon.Api.Endpoints.Webhooks;

namespace Aethon.Api.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapAuthEndpoints();
        api.MapAdminEndpoints();
        api.MapImportEndpoints();
        api.MapPublicEndpoints();
        api.MapJobEndpoints();
        api.MapApplicationEndpointsGroup();
        api.MapCandidateEndpoints();
        api.MapFileEndpoints();
        api.MapIntegrationEndpoints();
        api.MapIdentityEndpoints();
        api.MapOrganisationEndpoints();
        api.MapRecruiterCompanyEndpoints();
        api.MapCompanyRecruiterEndpoints();
        api.MapRecruiterJobEndpoints();
        api.MapCompanyJobEndpoints();
        api.MapBillingEndpoints();
        api.MapStripeWebhookEndpoints();
    }
}
