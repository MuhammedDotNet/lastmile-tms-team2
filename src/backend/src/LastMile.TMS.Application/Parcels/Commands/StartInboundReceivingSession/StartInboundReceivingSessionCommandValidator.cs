using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class StartInboundReceivingSessionCommandValidator : AbstractValidator<StartInboundReceivingSessionCommand>
{
    public StartInboundReceivingSessionCommandValidator()
    {
        RuleFor(x => x.ManifestId)
            .NotEmpty()
            .WithMessage("ManifestId is required.");
    }
}
