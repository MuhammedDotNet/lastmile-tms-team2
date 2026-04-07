using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class ScanInboundParcelCommandValidator : AbstractValidator<ScanInboundParcelCommand>
{
    public ScanInboundParcelCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required.");

        RuleFor(x => x.Barcode)
            .NotEmpty()
            .WithMessage("Barcode is required.")
            .MaximumLength(100)
            .WithMessage("Barcode must not exceed 100 characters.");
    }
}
