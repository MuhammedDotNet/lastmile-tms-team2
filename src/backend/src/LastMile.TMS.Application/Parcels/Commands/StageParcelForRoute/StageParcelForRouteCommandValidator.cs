using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class StageParcelForRouteCommandValidator : AbstractValidator<StageParcelForRouteCommand>
{
    public StageParcelForRouteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty();

        RuleFor(x => x.Barcode)
            .NotEmpty()
            .MaximumLength(100);
    }
}
