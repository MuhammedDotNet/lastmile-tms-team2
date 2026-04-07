using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class ConfirmInboundReceivingSessionCommandValidator : AbstractValidator<ConfirmInboundReceivingSessionCommand>
{
    public ConfirmInboundReceivingSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required.");
    }
}
