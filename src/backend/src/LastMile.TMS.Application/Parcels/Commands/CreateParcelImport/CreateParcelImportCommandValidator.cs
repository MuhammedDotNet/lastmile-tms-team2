using FluentValidation;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class CreateParcelImportCommandValidator : AbstractValidator<CreateParcelImportCommand>
{
    public CreateParcelImportCommandValidator()
    {
        RuleFor(x => x.Dto.ShipperAddressId)
            .NotEmpty().WithMessage("ShipperAddressId is required.");

        RuleFor(x => x.Dto.FileName)
            .NotEmpty().WithMessage("FileName is required.");

        RuleFor(x => x.Dto.SourceFile)
            .NotEmpty().WithMessage("SourceFile is required.");
    }
}
