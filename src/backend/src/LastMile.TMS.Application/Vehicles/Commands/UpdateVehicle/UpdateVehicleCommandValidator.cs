using FluentValidation;

namespace LastMile.TMS.Application.Vehicles.Commands;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.RegistrationPlate)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty();

        RuleFor(x => x.Dto.ParcelCapacity)
            .GreaterThan(0);

        RuleFor(x => x.Dto.WeightCapacity)
            .GreaterThan(0);
    }
}
