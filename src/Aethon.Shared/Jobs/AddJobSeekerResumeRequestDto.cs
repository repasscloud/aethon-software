using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Jobs;

public sealed class AddJobSeekerResumeRequestDto
{
    public Guid FileId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; }
}
