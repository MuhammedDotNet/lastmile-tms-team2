using FluentValidation;
using LastMile.TMS.Application.Drivers.DTOs;

namespace LastMile.TMS.Application.Drivers.Commands;

public sealed class UpdateDriverCommandValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Dto.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Dto.LicenseNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Dto.ZoneId)
            .NotEmpty();

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty();

        RuleFor(x => x.Dto.UserId)
            .NotEmpty();

        RuleFor(x => x.Dto.AvailabilitySchedule)
            .Must(HaveUniqueDaysOfWeek)
            .WithMessage("Each day of the week can appear only once in the availability schedule.");
    }

    private static bool HaveUniqueDaysOfWeek(
        IReadOnlyCollection<UpdateDriverAvailabilityDto> schedule)
    {
        if (schedule.Count == 0)
            return true;

        return schedule.Select(x => x.DayOfWeek).Distinct().Count() == schedule.Count;
    }
}
