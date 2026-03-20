using Aethon.Shared.Files;

namespace Aethon.Shared.Jobs;

public sealed class JobSeekerResumeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public StoredFileDto File { get; set; } = new();
}
