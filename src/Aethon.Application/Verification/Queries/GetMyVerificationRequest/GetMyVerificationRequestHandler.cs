using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Verification;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Verification.Queries.GetMyVerificationRequest;

public sealed class GetMyVerificationRequestHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetMyVerificationRequestHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<MyVerificationRequestDto?>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<MyVerificationRequestDto?>.Failure("auth.unauthenticated", "Not authenticated.");

        var latest = await _db.IdentityVerificationRequests
            .AsNoTracking()
            .Where(r => r.UserId == _currentUser.UserId)
            .OrderByDescending(r => r.CreatedUtc)
            .Select(r => new MyVerificationRequestDto
            {
                Id = r.Id,
                Status = r.Status.ToString(),
                RequestedUtc = r.CreatedUtc,
                ReviewedUtc = r.ReviewedUtc,
                ReviewNotes = r.ReviewNotes
            })
            .FirstOrDefaultAsync(ct);

        return Result<MyVerificationRequestDto?>.Success(latest);
    }
}
