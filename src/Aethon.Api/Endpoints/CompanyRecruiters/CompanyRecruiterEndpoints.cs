using Aethon.Api.Common;
using Aethon.Application.CompanyRecruiters.Commands.ApproveCompanyRecruiter;
using Aethon.Application.CompanyRecruiters.Commands.CreateCompanyRecruiterInvite;
using Aethon.Application.CompanyRecruiters.Commands.RejectCompanyRecruiter;
using Aethon.Application.CompanyRecruiters.Commands.SuspendCompanyRecruiter;
using Aethon.Application.CompanyRecruiters.Queries.GetCompanyRecruiters;
using Aethon.Application.CompanyRecruiters.Queries.GetPendingCompanyRecruiters;
using Aethon.Shared.CompanyRecruiters;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.CompanyRecruiters;

public static class CompanyRecruiterEndpoints
{
    public static void MapCompanyRecruiterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company/recruiters")
            .RequireAuthorization()
            .WithTags("CompanyRecruiters");

        group.MapGet(string.Empty, async (
            [FromServices] GetCompanyRecruitersHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/pending", async (
            [FromServices] GetPendingCompanyRecruitersHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/invite", async (
            [FromServices] CreateCompanyRecruiterInviteHandler handler,
            HttpContext ctx,
            CreateCompanyRecruiterInviteDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{partnershipId:guid}/approve", async (
            [FromServices] ApproveCompanyRecruiterHandler handler,
            HttpContext ctx,
            Guid partnershipId,
            ApproveRecruiterCompanyRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(partnershipId, request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{partnershipId:guid}/reject", async (
            [FromServices] RejectCompanyRecruiterHandler handler,
            HttpContext ctx,
            Guid partnershipId,
            RejectRecruiterCompanyRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(partnershipId, request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{partnershipId:guid}/suspend", async (
            [FromServices] SuspendCompanyRecruiterHandler handler,
            Guid partnershipId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(partnershipId, ct);
            return result.ToMinimalApiResult();
        });
    }
}
