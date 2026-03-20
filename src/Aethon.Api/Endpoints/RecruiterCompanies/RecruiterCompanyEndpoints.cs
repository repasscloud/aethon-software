using Aethon.Api.Common;
using Aethon.Application.RecruiterCompanies.Commands.CancelRecruiterCompanyRequest;
using Aethon.Application.RecruiterCompanies.Commands.CreateRecruiterCompanyRequest;
using Aethon.Application.RecruiterCompanies.Queries.GetRecruiterCompanies;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.RecruiterCompanies;

public static class RecruiterCompanyEndpoints
{
    public static void MapRecruiterCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recruiter/companies")
            .RequireAuthorization()
            .WithTags("RecruiterCompanies");

        group.MapGet(string.Empty, async (
            [FromServices] GetRecruiterCompaniesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/requests", async (
            [FromServices] CreateRecruiterCompanyRequestHandler handler,
            HttpContext ctx,
            CreateRecruiterCompanyRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapDelete("/requests/{partnershipId:guid}", async (
            [FromServices] CancelRecruiterCompanyRequestHandler handler,
            Guid partnershipId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(partnershipId, ct);
            return result.ToMinimalApiResult();
        });
    }
}
