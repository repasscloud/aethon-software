using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class CandidateLanguageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LanguageAbilityType AbilityType { get; set; }
    public LanguageProficiencyLevel? ProficiencyLevel { get; set; }
    public bool IsVerified { get; set; }
}
