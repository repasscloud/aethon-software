using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.UpdateMyDisplayName;

public sealed class UpdateMyDisplayNameHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public UpdateMyDisplayNameHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(UpdateDisplayNameRequestDto request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var displayName = request.DisplayName?.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure("member_profile.invalid_display_name", "Display name cannot be empty.");

        if (displayName.Length > 150)
            return Result.Failure("member_profile.invalid_display_name", "Display name must be 150 characters or fewer.");

        var updated = await _db.Users
            .Where(u => u.Id == _currentUser.UserId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.DisplayName, displayName),
                ct);

        if (updated == 0)
            return Result.Failure("auth.unauthenticated", "User account not found.");

        return Result.Success();
    }
}
