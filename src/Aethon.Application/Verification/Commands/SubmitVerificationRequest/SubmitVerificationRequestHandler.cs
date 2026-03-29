using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Verification;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Verification.Commands.SubmitVerificationRequest;

public sealed class SubmitVerificationRequestHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public SubmitVerificationRequestHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<MyVerificationRequestDto>> HandleAsync(
        SubmitVerificationRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<MyVerificationRequestDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == _currentUser.UserId)
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Result<MyVerificationRequestDto>.Failure("auth.unauthenticated", "User not found.");

        if (!user.EmailConfirmed)
            return Result<MyVerificationRequestDto>.Failure("verification.email_not_confirmed",
                "You must confirm your email address before requesting identity verification.");

        if (user.IsIdentityVerified)
            return Result<MyVerificationRequestDto>.Failure("verification.already_verified",
                "Your identity is already verified.");

        var hasPending = await _db.IdentityVerificationRequests
            .AnyAsync(r => r.UserId == _currentUser.UserId && r.Status == VerificationRequestStatus.Pending, ct);

        if (hasPending)
            return Result<MyVerificationRequestDto>.Failure("verification.request_pending",
                "You already have a pending verification request.");

        var entity = new IdentityVerificationRequest
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            FullName = request.FullName.Trim(),
            EmailAddress = request.EmailAddress.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AdditionalNotes = request.AdditionalNotes?.Trim(),
            Status = VerificationRequestStatus.Pending,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _db.IdentityVerificationRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Result<MyVerificationRequestDto>.Success(new MyVerificationRequestDto
        {
            Id = entity.Id,
            Status = entity.Status.ToString(),
            RequestedUtc = entity.CreatedUtc
        });
    }
}
