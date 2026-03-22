using Aethon.Api.Common;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Jobs.Commands.EmailJobApplication;
using Aethon.Application.Jobs.Queries.GetPublicJobDetail;
using Aethon.Application.Jobs.Queries.GetPublicJobLocations;
using Aethon.Application.Jobs.Queries.GetPublicJobs;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationProfile;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.Public;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/public")
            .AllowAnonymous()
            .WithTags("Public");

        // GET /api/v1/public/organisations/{slug}
        group.MapGet("/organisations/{slug}", async (
            [FromServices] GetPublicOrganisationProfileHandler handler,
            string slug,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs/locations?q= — typeahead suggestions from active jobs
        group.MapGet("/jobs/locations", async (
            [FromServices] GetPublicJobLocationsHandler handler,
            string? q,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(q, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs
        group.MapGet("/jobs", async (
            [FromServices] GetPublicJobsHandler handler,
            string? city,
            DateRangeFilter? dateRange,
            JobCategory? category,
            JobRegion? region,
            string? country,
            string? keywords,
            string? organisationSlug,
            decimal? salaryMin,
            decimal? salaryMax,
            bool? verifiedOnly,
            WorkplaceType? workplaceType,
            bool? immediateStart,
            CancellationToken ct) =>
        {
            var query = new GetPublicJobsQuery
            {
                City = city,
                DateRange = dateRange,
                Category = category,
                Region = region,
                Country = country,
                Keywords = keywords,
                OrganisationSlug = organisationSlug,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax,
                VerifiedOnly = verifiedOnly ?? false,
                WorkplaceType = workplaceType,
                ImmediateStart = immediateStart ?? false
            };
            var result = await handler.HandleAsync(query, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs/{id}
        group.MapGet("/jobs/{jobId:guid}", async (
            [FromServices] GetPublicJobDetailHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/public/jobs/{id}/apply-email  (anonymous — sends CV by email to employer)
        group.MapPost("/jobs/{jobId:guid}/apply-email", async (
            [FromServices] EmailJobApplicationHandler handler,
            Guid jobId,
            EmailJobApplicationRequestDto request,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, request, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/public/jobs/{id}/apply  (requires auth)
        group.MapPost("/jobs/{jobId:guid}/apply", async (
            [FromServices] SubmitJobApplicationHandler handler,
            HttpContext ctx,
            Guid jobId,
            CreateJobApplicationRequestDto request,
            CancellationToken ct) =>
        {
            var command = new SubmitJobApplicationCommand
            {
                JobId = jobId,
                CoverLetter = request.CoverLetter,
                Source = request.Source ?? "AethonPublicBoard",
                ScreeningAnswersJson = request.ScreeningAnswersJson
            };

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        }).RequireAuthorization();
    }
}
