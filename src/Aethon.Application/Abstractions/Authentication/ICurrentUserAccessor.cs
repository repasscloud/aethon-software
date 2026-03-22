namespace Aethon.Application.Abstractions.Authentication;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsStaff { get; }
}