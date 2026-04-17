namespace Aethon.Shared.Organisations;

public sealed class OrganisationMembersResponseDto
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = "";
    public string OrganisationType { get; set; } = "";
    public bool IsOwner { get; set; }
    public bool CanInvite { get; set; }
    public List<OrganisationMemberDto> Members { get; set; } = [];
    public List<OrganisationInviteDto> PendingInvites { get; set; } = [];
}
