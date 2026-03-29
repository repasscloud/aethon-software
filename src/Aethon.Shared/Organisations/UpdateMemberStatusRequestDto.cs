namespace Aethon.Shared.Organisations;

public sealed class UpdateMemberStatusRequestDto
{
    /// <summary>Active | Suspended | Revoked</summary>
    public string Status { get; set; } = null!;
}
