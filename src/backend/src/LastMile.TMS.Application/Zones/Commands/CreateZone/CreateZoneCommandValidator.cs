using FluentValidation;

namespace LastMile.TMS.Application.Zones.Commands;

public sealed class CreateZoneCommandValidator : AbstractValidator<CreateZoneCommand>
{
    public CreateZoneCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Zone name is required.")
            .MaximumLength(200).WithMessage("Zone name must not exceed 200 characters.");

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty().WithMessage("DepotId is required.");

        RuleFor(x => x)
            .Must(HaveExactlyOneBoundarySource)
            .WithMessage("Provide exactly one of: geoJson, coordinates, or boundaryWkt.");
    }

    private static bool HaveExactlyOneBoundarySource(CreateZoneCommand cmd)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(cmd.Dto.GeoJson)) count++;
        if (cmd.Dto.Coordinates is { Count: > 0 }) count++;
        if (!string.IsNullOrWhiteSpace(cmd.Dto.BoundaryWkt)) count++;
        return count == 1;
    }
}
