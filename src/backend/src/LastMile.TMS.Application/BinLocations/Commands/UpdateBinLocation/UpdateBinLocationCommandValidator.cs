using FluentValidation;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class UpdateBinLocationCommandValidator : AbstractValidator<UpdateBinLocationCommand>
{
    public UpdateBinLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Bin location name is required.")
            .MaximumLength(200).WithMessage("Bin location name must not exceed 200 characters.");

        RuleFor(x => x.Dto.StorageAisleId)
            .NotEmpty().WithMessage("StorageAisleId is required.");
    }
}
