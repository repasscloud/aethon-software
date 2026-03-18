using Aethon.Application.Common.Results;
using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Services;

public sealed class ApplicationWorkflowService
{
    public Result ValidateStatusChange(
        ApplicationStatus currentStatus,
        ApplicationStatus nextStatus,
        string? reason)
    {
        if (currentStatus == nextStatus)
        {
            return Result.Failure(
                "applications.status.no_change",
                "The application is already in the requested status.");
        }

        if (!IsTransitionAllowed(currentStatus, nextStatus))
        {
            return Result.Failure(
                "applications.status.invalid_transition",
                $"The status cannot move from {currentStatus} to {nextStatus}.");
        }

        if ((nextStatus == ApplicationStatus.Rejected || nextStatus == ApplicationStatus.Withdrawn) &&
            string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(
                "applications.status.reason_required",
                "A reason is required when rejecting or withdrawing an application.");
        }

        return Result.Success();
    }

    private static bool IsTransitionAllowed(ApplicationStatus currentStatus, ApplicationStatus nextStatus)
    {
        return currentStatus switch
        {
            ApplicationStatus.Draft => nextStatus is
                ApplicationStatus.Submitted or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Submitted => nextStatus is
                ApplicationStatus.UnderReview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.UnderReview => nextStatus is
                ApplicationStatus.Shortlisted or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Shortlisted => nextStatus is
                ApplicationStatus.Interview or
                ApplicationStatus.UnderReview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Interview => nextStatus is
                ApplicationStatus.Offer or
                ApplicationStatus.Shortlisted or
                ApplicationStatus.UnderReview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Offer => nextStatus is
                ApplicationStatus.Hired or
                ApplicationStatus.UnderReview or
                ApplicationStatus.Rejected or
                ApplicationStatus.Withdrawn,

            ApplicationStatus.Hired => false,
            ApplicationStatus.Rejected => false,
            ApplicationStatus.Withdrawn => false,
            _ => false
        };
    }
}
