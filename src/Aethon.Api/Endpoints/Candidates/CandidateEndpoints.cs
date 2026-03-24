using Aethon.Api.Common;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Candidates.Commands.TriggerResumeAnalysis;
using Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetResumeAnalysis;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Candidates;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Candidates;

public static class CandidateEndpoints
{
    public static void MapCandidateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/me")
            .RequireAuthorization()
            .WithTags("Candidates");

        group.MapGet("/profile", async (
            [FromServices] GetMyCandidateProfileHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetMyCandidateProfileQuery(), ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/me/resumes/{resumeId}/analysis
        group.MapGet("/resumes/{resumeId:guid}/analysis", async (
            [FromServices] GetResumeAnalysisHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(resumeId, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/me/resumes/{resumeId}/analysis/trigger
        group.MapPost("/resumes/{resumeId:guid}/analysis/trigger", async (
            [FromServices] TriggerResumeAnalysisHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(resumeId, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/me/profile/check-slug?slug=...
        group.MapGet("/profile/check-slug", async (
            AethonDbContext db,
            ICurrentUserAccessor currentUser,
            string slug,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Results.BadRequest(new { available = false, message = "Slug is required." });

            var normalised = slug.Trim().ToLowerInvariant();
            var taken = await db.JobSeekerProfiles
                .AnyAsync(p => p.Slug == normalised && p.UserId != currentUser.UserId, ct);

            return Results.Ok(new { available = !taken, slug = normalised });
        });

        group.MapPut("/profile", async (
            [FromServices] UpsertMyCandidateProfileHandler handler,
            HttpContext httpContext,
            UpsertMyCandidateProfileCommand command,
            CancellationToken ct) =>
        {
            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        // ── Work Experience ──────────────────────────────────────────────────────────

        group.MapGet("/profile/work-experiences", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles
                .Include(p => p.WorkExperiences)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.Ok(Array.Empty<object>());
            var list = profile.WorkExperiences
                .OrderBy(x => x.SortOrder).ThenByDescending(x => x.StartYear).ThenByDescending(x => x.StartMonth)
                .Select(x => new JobSeekerWorkExperienceDto
                {
                    Id = x.Id, JobTitle = x.JobTitle, EmployerName = x.EmployerName,
                    StartMonth = x.StartMonth, StartYear = x.StartYear,
                    EndMonth = x.EndMonth, EndYear = x.EndYear,
                    IsCurrent = x.IsCurrent, Description = x.Description, SortOrder = x.SortOrder
                });
            return Results.Ok(list);
        });

        group.MapPost("/profile/work-experiences", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            JobSeekerWorkExperienceDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = new JobSeekerWorkExperience
            {
                Id = Guid.NewGuid(), JobSeekerProfileId = profile.Id,
                JobTitle = dto.JobTitle, EmployerName = dto.EmployerName,
                StartMonth = dto.StartMonth, StartYear = dto.StartYear,
                EndMonth = dto.EndMonth, EndYear = dto.EndYear,
                IsCurrent = dto.IsCurrent, Description = dto.Description,
                SortOrder = dto.SortOrder,
                CreatedUtc = DateTime.UtcNow, CreatedByUserId = currentUser.UserId
            };
            db.JobSeekerWorkExperiences.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = entity.Id });
        });

        group.MapPut("/profile/work-experiences/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            Guid id, JobSeekerWorkExperienceDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = await db.JobSeekerWorkExperiences.FirstOrDefaultAsync(x => x.Id == id && x.JobSeekerProfileId == profile.Id, ct);
            if (entity is null) return Results.NotFound();
            entity.JobTitle = dto.JobTitle; entity.EmployerName = dto.EmployerName;
            entity.StartMonth = dto.StartMonth; entity.StartYear = dto.StartYear;
            entity.EndMonth = dto.EndMonth; entity.EndYear = dto.EndYear;
            entity.IsCurrent = dto.IsCurrent; entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder; entity.UpdatedUtc = DateTime.UtcNow; entity.UpdatedByUserId = currentUser.UserId;
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });

        group.MapDelete("/profile/work-experiences/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, Guid id, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            await db.JobSeekerWorkExperiences
                .Where(x => x.Id == id && x.JobSeekerProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);
            return Results.Ok();
        });

        // ── Qualifications ───────────────────────────────────────────────────────────

        group.MapGet("/profile/qualifications", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles
                .Include(p => p.Qualifications)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.Ok(Array.Empty<object>());
            var list = profile.Qualifications
                .OrderBy(x => x.SortOrder)
                .Select(x => new JobSeekerQualificationDto
                {
                    Id = x.Id, Title = x.Title, Institution = x.Institution,
                    CompletedYear = x.CompletedYear, Description = x.Description, SortOrder = x.SortOrder
                });
            return Results.Ok(list);
        });

        group.MapPost("/profile/qualifications", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            JobSeekerQualificationDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = new JobSeekerQualification
            {
                Id = Guid.NewGuid(), JobSeekerProfileId = profile.Id,
                Title = dto.Title, Institution = dto.Institution,
                CompletedYear = dto.CompletedYear, Description = dto.Description,
                SortOrder = dto.SortOrder,
                CreatedUtc = DateTime.UtcNow, CreatedByUserId = currentUser.UserId
            };
            db.JobSeekerQualifications.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = entity.Id });
        });

        group.MapPut("/profile/qualifications/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            Guid id, JobSeekerQualificationDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = await db.JobSeekerQualifications.FirstOrDefaultAsync(x => x.Id == id && x.JobSeekerProfileId == profile.Id, ct);
            if (entity is null) return Results.NotFound();
            entity.Title = dto.Title; entity.Institution = dto.Institution;
            entity.CompletedYear = dto.CompletedYear; entity.Description = dto.Description;
            entity.SortOrder = dto.SortOrder; entity.UpdatedUtc = DateTime.UtcNow; entity.UpdatedByUserId = currentUser.UserId;
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });

        group.MapDelete("/profile/qualifications/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, Guid id, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            await db.JobSeekerQualifications
                .Where(x => x.Id == id && x.JobSeekerProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);
            return Results.Ok();
        });

        // ── Certificates ─────────────────────────────────────────────────────────────

        group.MapGet("/profile/certificates", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles
                .Include(p => p.Certificates)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.Ok(Array.Empty<object>());
            var list = profile.Certificates
                .OrderBy(x => x.SortOrder)
                .Select(x => new JobSeekerCertificateDto
                {
                    Id = x.Id, Name = x.Name, IssuingOrganisation = x.IssuingOrganisation,
                    IssuedMonth = x.IssuedMonth, IssuedYear = x.IssuedYear, ExpiryYear = x.ExpiryYear,
                    CredentialId = x.CredentialId, CredentialUrl = x.CredentialUrl, SortOrder = x.SortOrder
                });
            return Results.Ok(list);
        });

        group.MapPost("/profile/certificates", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            JobSeekerCertificateDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = new JobSeekerCertificate
            {
                Id = Guid.NewGuid(), JobSeekerProfileId = profile.Id,
                Name = dto.Name, IssuingOrganisation = dto.IssuingOrganisation,
                IssuedMonth = dto.IssuedMonth, IssuedYear = dto.IssuedYear, ExpiryYear = dto.ExpiryYear,
                CredentialId = dto.CredentialId, CredentialUrl = dto.CredentialUrl,
                SortOrder = dto.SortOrder,
                CreatedUtc = DateTime.UtcNow, CreatedByUserId = currentUser.UserId
            };
            db.JobSeekerCertificates.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = entity.Id });
        });

        group.MapPut("/profile/certificates/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            Guid id, JobSeekerCertificateDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = await db.JobSeekerCertificates.FirstOrDefaultAsync(x => x.Id == id && x.JobSeekerProfileId == profile.Id, ct);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.IssuingOrganisation = dto.IssuingOrganisation;
            entity.IssuedMonth = dto.IssuedMonth; entity.IssuedYear = dto.IssuedYear; entity.ExpiryYear = dto.ExpiryYear;
            entity.CredentialId = dto.CredentialId; entity.CredentialUrl = dto.CredentialUrl;
            entity.SortOrder = dto.SortOrder; entity.UpdatedUtc = DateTime.UtcNow; entity.UpdatedByUserId = currentUser.UserId;
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });

        group.MapDelete("/profile/certificates/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, Guid id, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            await db.JobSeekerCertificates
                .Where(x => x.Id == id && x.JobSeekerProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);
            return Results.Ok();
        });

        // ── Skills ───────────────────────────────────────────────────────────────────

        group.MapGet("/profile/skills", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles
                .Include(p => p.Skills)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.Ok(Array.Empty<object>());
            var list = profile.Skills
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new JobSeekerSkillDto
                {
                    Id = x.Id, Name = x.Name, SkillLevel = x.SkillLevel, SortOrder = x.SortOrder
                });
            return Results.Ok(list);
        });

        group.MapPost("/profile/skills", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            JobSeekerSkillDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = new JobSeekerSkill
            {
                Id = Guid.NewGuid(), JobSeekerProfileId = profile.Id,
                Name = dto.Name, SkillLevel = dto.SkillLevel, SortOrder = dto.SortOrder,
                CreatedUtc = DateTime.UtcNow, CreatedByUserId = currentUser.UserId
            };
            db.JobSeekerSkills.Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = entity.Id });
        });

        group.MapPut("/profile/skills/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser,
            Guid id, JobSeekerSkillDto dto, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            var entity = await db.JobSeekerSkills.FirstOrDefaultAsync(x => x.Id == id && x.JobSeekerProfileId == profile.Id, ct);
            if (entity is null) return Results.NotFound();
            entity.Name = dto.Name; entity.SkillLevel = dto.SkillLevel;
            entity.SortOrder = dto.SortOrder; entity.UpdatedUtc = DateTime.UtcNow; entity.UpdatedByUserId = currentUser.UserId;
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });

        group.MapDelete("/profile/skills/{id:guid}", async (
            AethonDbContext db, ICurrentUserAccessor currentUser, Guid id, CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();
            await db.JobSeekerSkills
                .Where(x => x.Id == id && x.JobSeekerProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);
            return Results.Ok();
        });

        // POST /api/v1/me/profile/resume/{fileId} — link an uploaded file as the active resume
        group.MapPost("/profile/resume/{fileId:guid}", async (
            AethonDbContext db,
            ICurrentUserAccessor currentUser,
            Guid fileId,
            CancellationToken ct) =>
        {
            var profile = await db.JobSeekerProfiles
                .Include(p => p.Resumes)
                .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct);
            if (profile is null) return Results.NotFound();

            var storedFile = await db.StoredFiles.FirstOrDefaultAsync(f => f.Id == fileId, ct);
            if (storedFile is null) return Results.NotFound();

            // Deactivate old resumes
            foreach (var r in profile.Resumes) { r.IsActive = false; r.IsDefault = false; }

            var resume = new JobSeekerResume
            {
                Id = Guid.NewGuid(),
                JobSeekerProfileId = profile.Id,
                StoredFileId = fileId,
                Name = storedFile.OriginalFileName,
                IsDefault = true,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow,
                CreatedByUserId = currentUser.UserId
            };
            db.JobSeekerResumes.Add(resume);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = resume.Id });
        });
    }
}
