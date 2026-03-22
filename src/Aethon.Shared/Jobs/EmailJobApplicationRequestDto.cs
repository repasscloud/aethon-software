using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Jobs;

public sealed class EmailJobApplicationRequestDto
{
    [Required, MaxLength(120)]
    public string ApplicantName { get; set; } = "";

    [Required, EmailAddress, MaxLength(254)]
    public string ApplicantEmail { get; set; } = "";

    [MaxLength(50)]
    public string? ApplicantPhone { get; set; }

    [MaxLength(16000)]
    public string? CoverLetter { get; set; }

    /// <summary>File name of the attached resume/CV (e.g. "resume.pdf").</summary>
    [MaxLength(260)]
    public string? ResumeFileName { get; set; }

    /// <summary>Base64-encoded file content.</summary>
    public string? ResumeContentBase64 { get; set; }

    /// <summary>MIME type of the attached file.</summary>
    [MaxLength(100)]
    public string? ResumeContentType { get; set; }
}
