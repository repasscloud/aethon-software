using FluentValidation;

namespace Aethon.Application.Applications.Commands.ScheduleInterview;

public sealed class ScheduleInterviewValidator : AbstractValidator<ScheduleInterviewCommand>
{
    public ScheduleInterviewValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.ScheduledStartUtc)
            .NotEmpty();

        RuleFor(x => x.ScheduledEndUtc)
            .GreaterThan(x => x.ScheduledStartUtc);

        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .MaximumLength(250);

        RuleFor(x => x.MeetingUrl)
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
