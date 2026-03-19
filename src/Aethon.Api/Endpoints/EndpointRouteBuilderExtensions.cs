using Aethon.Api.Endpoints.Applications;
using Aethon.Api.Endpoints.Auth;
using Aethon.Api.Endpoints.Candidates;
using Aethon.Api.Endpoints.Jobs;
using Microsoft.AspNetCore.Builder;

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
    }
}
