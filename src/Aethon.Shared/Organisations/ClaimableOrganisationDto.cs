namespace Aethon.Shared.Organisations;

public sealed class ClaimableOrganisationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool HasActiveClaim { get; set; }
}
