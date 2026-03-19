namespace Aethon.Shared.Candidates;

public sealed class CandidateNationalityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
