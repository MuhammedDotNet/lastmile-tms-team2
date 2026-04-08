using FluentValidation;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class CreateBinLocationCommandValidator : AbstractValidator<CreateBinLocationCommand>
{
    public CreateBinLocationCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Bin location name is required.")
            .MaximumLength(200).WithMessage("Bin location name must not exceed 200 characters.");

        RuleFor(x => x.Dto.StorageAisleId)
            .NotEmpty().WithMessage("StorageAisleId is required.");

        RuleFor(x => x.Dto.DeliveryZoneId)
            .NotEmpty().WithMessage("DeliveryZoneId must not be empty when provided.")
            .When(x => x.Dto.DeliveryZoneId.HasValue);
    }
}
