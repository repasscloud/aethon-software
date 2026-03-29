using Aethon.Application.Abstractions.Email;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Aethon.Application.Jobs.Commands.EmailJobApplication;

public sealed class EmailJobApplicationHandler
{
    private readonly AethonDbContext _db;
    private readonly IEmailService _emailService;

    public EmailJobApplicationHandler(AethonDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<Result> HandleAsync(Guid jobId, EmailJobApplicationRequestDto request, CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;

        var job = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Id == jobId
                     && j.Status == JobStatus.Published
                     && j.Visibility == JobVisibility.Public
                     && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow)
                     && j.ApplicationEmail != null)
            .Select(j => new
            {
                j.Title,
                j.ApplicationEmail,
                OrgName = j.OwnedByOrganisation.Name
            })
            .FirstOrDefaultAsync(ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "Job not found or does not accept email applications.");

        var attachments = new List<EmailAttachment>();
        if (!string.IsNullOrWhiteSpace(request.ResumeFileName)
            && !string.IsNullOrWhiteSpace(request.ResumeContentBase64))
        {
            attachments.Add(new EmailAttachment
            {
                FileName = request.ResumeFileName,
                ContentBase64 = request.ResumeContentBase64,
                ContentType = string.IsNullOrWhiteSpace(request.ResumeContentType)
                    ? "application/octet-stream"
                    : request.ResumeContentType
            });
        }

        var subject = $"Job Application: {job.Title} — {request.ApplicantName}";

        var text = BuildTextBody(job.Title, job.OrgName, request);
        var html = BuildHtmlBody(job.Title, job.OrgName, request);

        var message = new EmailMessage
        {
            ToEmail = job.ApplicationEmail!,
            Subject = subject,
            TextBody = text,
            HtmlBody = html,
            ReplyToEmail = request.ApplicantEmail,
            ReplyToName = request.ApplicantName,
            Attachments = attachments
        };

        await _emailService.SendAsync(message, ct);

        return Result.Success();
    }

    private static string BuildTextBody(string jobTitle, string orgName, EmailJobApplicationRequestDto r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"New application for: {jobTitle} at {orgName}");
        sb.AppendLine();
        sb.AppendLine($"Applicant: {r.ApplicantName}");
        sb.AppendLine($"Email: {r.ApplicantEmail}");
        if (!string.IsNullOrWhiteSpace(r.ApplicantPhone))
            sb.AppendLine($"Phone: {r.ApplicantPhone}");
        if (!string.IsNullOrWhiteSpace(r.CoverLetter))
        {
            sb.AppendLine();
            sb.AppendLine("Cover letter:");
            sb.AppendLine(r.CoverLetter);
        }
        if (!string.IsNullOrWhiteSpace(r.ResumeFileName))
        {
            sb.AppendLine();
            sb.AppendLine($"Resume attached: {r.ResumeFileName}");
        }
        return sb.ToString();
    }

    private static string BuildHtmlBody(string jobTitle, string orgName, EmailJobApplicationRequestDto r)
    {
        var coverHtml = string.IsNullOrWhiteSpace(r.CoverLetter)
            ? ""
            : $"<h3>Cover letter</h3><p style=\"white-space:pre-wrap\">{System.Net.WebUtility.HtmlEncode(r.CoverLetter)}</p>";

        var resumeLine = string.IsNullOrWhiteSpace(r.ResumeFileName)
            ? ""
            : $"<p><strong>Resume attached:</strong> {System.Net.WebUtility.HtmlEncode(r.ResumeFileName)}</p>";

        var phoneLine = string.IsNullOrWhiteSpace(r.ApplicantPhone)
            ? ""
            : $"<p><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(r.ApplicantPhone)}</p>";

        return $"""
            <p><strong>New application for:</strong> {System.Net.WebUtility.HtmlEncode(jobTitle)} at {System.Net.WebUtility.HtmlEncode(orgName)}</p>
            <hr />
            <p><strong>Applicant:</strong> {System.Net.WebUtility.HtmlEncode(r.ApplicantName)}</p>
            <p><strong>Email:</strong> <a href="mailto:{System.Net.WebUtility.HtmlEncode(r.ApplicantEmail)}">{System.Net.WebUtility.HtmlEncode(r.ApplicantEmail)}</a></p>
            {phoneLine}
            {coverHtml}
            {resumeLine}
            """;
    }
}
