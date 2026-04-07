using FluentValidation;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class CreateStorageZoneCommandValidator : AbstractValidator<CreateStorageZoneCommand>
{
    public CreateStorageZoneCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Storage zone name is required.")
            .MaximumLength(200).WithMessage("Storage zone name must not exceed 200 characters.");

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty().WithMessage("DepotId is required.");
    }
}
