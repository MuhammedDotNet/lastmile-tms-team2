using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class CancelParcelCommandValidator : AbstractValidator<CancelParcelCommand>
{
    public CancelParcelCommandValidator()
    {
        RuleFor(command => command.ParcelId)
            .NotEmpty().WithMessage("Parcel id is required.");

        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage("Cancel reason is required.")
            .Must(reason => !string.IsNullOrWhiteSpace(reason))
            .WithMessage("Cancel reason is required.")
            .MaximumLength(1000).WithMessage("Cancel reason must not exceed 1000 characters.");
    }
}
