using FluentValidation;

namespace LastMile.TMS.Application.Routes.Commands;

public sealed class CancelRouteCommandValidator : AbstractValidator<CancelRouteCommand>
{
    public CancelRouteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.Reason)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
