using Aethon.Api.Endpoints.Applications;
using Aethon.Api.Endpoints.Auth;
using Aethon.Api.Endpoints.Candidates;
using Aethon.Api.Endpoints.CompanyJobs;
using Aethon.Api.Endpoints.CompanyRecruiters;
using Aethon.Api.Endpoints.Files;
using Aethon.Api.Endpoints.Integrations;
using Aethon.Api.Endpoints.Jobs;
using Aethon.Api.Endpoints.Organisations;
using Aethon.Api.Endpoints.RecruiterCompanies;
using Aethon.Api.Endpoints.RecruiterJobs;

namespace Aethon.Api.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1");

        api.MapAuthEndpoints();
        api.MapJobEndpoints();
        api.MapApplicationEndpointsGroup();
        api.MapCandidateEndpoints();
        api.MapFileEndpoints();
        api.MapIntegrationEndpoints();
        api.MapOrganisationEndpoints();
        api.MapRecruiterCompanyEndpoints();
        api.MapCompanyRecruiterEndpoints();
        api.MapRecruiterJobEndpoints();
        api.MapCompanyJobEndpoints();
    }
}
