using FluentValidation;
using LastMile.TMS.Application.Routes.Commands;

namespace LastMile.TMS.Application.Routes.Validators;

public class CreateRouteCommandValidator : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteCommandValidator()
    {
        RuleFor(x => x.Dto.VehicleId)
            .NotEmpty();

        RuleFor(x => x.Dto.DriverId)
            .NotEmpty();

        RuleFor(x => x.Dto.StartMileage)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dto.ParcelIds)
            .NotNull()
            .NotEmpty();
    }
}
