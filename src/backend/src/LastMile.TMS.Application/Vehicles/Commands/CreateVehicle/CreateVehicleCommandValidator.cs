using FluentValidation;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Vehicles.Commands;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Dto.RegistrationPlate)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty();

        RuleFor(x => x.Dto.ParcelCapacity)
            .GreaterThan(0);

        RuleFor(x => x.Dto.WeightCapacity)
            .GreaterThan(0);

        RuleFor(x => x.Dto.Status)
            .Equal(VehicleStatus.Available)
            .WithMessage("New vehicles must be created with status Available.");
    }
}
