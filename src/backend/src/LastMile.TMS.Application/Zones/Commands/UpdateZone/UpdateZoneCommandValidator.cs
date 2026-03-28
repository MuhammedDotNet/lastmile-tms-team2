using FluentValidation;

namespace LastMile.TMS.Application.Zones.Commands;

public sealed class UpdateZoneCommandValidator : AbstractValidator<UpdateZoneCommand>
{
    public UpdateZoneCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Zone ID is required.");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Zone name is required.")
            .MaximumLength(200).WithMessage("Zone name must not exceed 200 characters.");

        RuleFor(x => x.Dto.DepotId)
            .NotEmpty().WithMessage("DepotId is required.");

        RuleFor(x => x)
            .Must(HaveAtMostOneBoundarySource)
            .WithMessage("Provide at most one of: geoJson, coordinates, or boundaryWkt. Use null for all three to leave the boundary unchanged.");
    }

    private static bool HaveAtMostOneBoundarySource(UpdateZoneCommand cmd)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(cmd.Dto.GeoJson)) count++;
        if (cmd.Dto.Coordinates is { Count: > 0 }) count++;
        if (!string.IsNullOrWhiteSpace(cmd.Dto.BoundaryWkt)) count++;
        return count <= 1;
    }
}
