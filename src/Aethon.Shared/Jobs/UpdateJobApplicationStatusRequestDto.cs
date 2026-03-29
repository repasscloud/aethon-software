using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Jobs;

public sealed class UpdateJobApplicationStatusRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Status { get; set; } = "";

    [MaxLength(4000)]
    public string? Notes { get; set; }
}
