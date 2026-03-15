using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Jobs;

public sealed class CreateJobApplicationRequestDto
{
    [Required]
    [MaxLength(4000)]
    public string? CoverLetter { get; set; }

    [MaxLength(250)]
    public string? Source { get; set; }
}
