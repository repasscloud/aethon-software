using Aethon.Application.Common.Results;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Services;

public sealed class ApplicationWorkflowService
{
    public Result ValidateStatusTransition(
        ApplicationStatus current,
        ApplicationStatus next,
        string? reason = null)
    {
        if (current == next)
        {
            return Result.Failure(
                "applications.status.no_change",
                "The application is already in the requested status.");
        }

        var allowed = current switch
        {
            ApplicationStatus.Draft => next is
                ApplicationStatus.Submitted or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Submitted => next is
                ApplicationStatus.UnderReview or
                ApplicationStatus.Shortlisted or
                ApplicationStatus.Interview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.UnderReview => next is
                ApplicationStatus.Shortlisted or
                ApplicationStatus.Interview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Shortlisted => next is
                ApplicationStatus.UnderReview or
                ApplicationStatus.Interview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Interview => next is
                ApplicationStatus.Shortlisted or
                ApplicationStatus.UnderReview or
                ApplicationStatus.Offer or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Offer => next is
                ApplicationStatus.Interview or
                ApplicationStatus.UnderReview or
                ApplicationStatus.Hired or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Hired => false,
            ApplicationStatus.Rejected => false,
            ApplicationStatus.Withdrawn => false,
            _ => false
        };

        if (!allowed)
        {
            return Result.Failure(
                "applications.status.invalid_transition",
                $"Cannot move application from '{current}' to '{next}'.");
        }

        if ((next == ApplicationStatus.Rejected || next == ApplicationStatus.Withdrawn) &&
            string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(
                "applications.status.reason_required",
                "A reason is required for rejected or withdrawn applications.");
        }

        return Result.Success();
    }

    public void ApplyStatusSideEffects(
        JobApplication application,
        ApplicationStatus nextStatus,
        string? reason,
        DateTime changedUtc,
        Guid? changedByUserId = null)
    {
        application.Status = nextStatus;
        application.StatusReason = reason;
        application.LastStatusChangedUtc = changedUtc;
        application.LastActivityUtc = changedUtc;
        application.UpdatedUtc = changedUtc;

        application.IsRejected = false;
        application.RejectedUtc = null;
        application.RejectionReason = null;
        application.RejectedByUserId = null;

        application.IsWithdrawn = false;
        application.WithdrawnUtc = null;
        application.WithdrawalReason = null;
        application.WithdrawnByUserId = null;

        application.IsHired = false;
        application.HiredUtc = null;

        switch (nextStatus)
        {
            case ApplicationStatus.Rejected:
                application.IsRejected = true;
                application.RejectedUtc = changedUtc;
                application.RejectionReason = reason;
                application.RejectedByUserId = changedByUserId;
                break;

            case ApplicationStatus.Withdrawn:
                application.IsWithdrawn = true;
                application.WithdrawnUtc = changedUtc;
                application.WithdrawalReason = reason;
                application.WithdrawnByUserId = changedByUserId;
                break;

            case ApplicationStatus.Hired:
                application.IsHired = true;
                application.HiredUtc = changedUtc;
                break;
        }
    }
}
