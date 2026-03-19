using Microsoft.AspNetCore.Builder;

namespace Aethon.Api.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapJobEndpoints();
        app.MapApplicationEndpointsGroup();
        app.MapCandidateEndpoints();
    }
}
