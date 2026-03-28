using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Zones.Mappings;
using LastMile.TMS.Application.Zones.Services;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Zones.Commands;

public sealed class UpdateZoneCommandHandler(
    IAppDbContext db,
    IZoneBoundaryParser boundaryParser)
    : IRequestHandler<UpdateZoneCommand, Zone?>
{
    public async Task<Zone?> Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await db.Zones
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

        if (zone is null)
            return null;

        request.Dto.UpdateEntity(zone);

        if (HasBoundaryInput(request))
        {
            var polygon = ParseBoundary(request);
            if (polygon is null)
                throw new ArgumentException("Failed to parse zone boundary from the provided input.");
            zone.Boundary = polygon;
        }

        await db.SaveChangesAsync(cancellationToken);
        return zone;
    }

    private static bool HasBoundaryInput(UpdateZoneCommand request)
    {
        return !string.IsNullOrWhiteSpace(request.Dto.GeoJson)
            || request.Dto.Coordinates is { Count: > 0 }
            || !string.IsNullOrWhiteSpace(request.Dto.BoundaryWkt);
    }

    private NetTopologySuite.Geometries.Polygon? ParseBoundary(UpdateZoneCommand request)
    {
        if (!string.IsNullOrWhiteSpace(request.Dto.GeoJson))
            return boundaryParser.ParseGeoJson(request.Dto.GeoJson);
        if (request.Dto.Coordinates is { Count: > 0 })
            return boundaryParser.ParseCoordinates(request.Dto.Coordinates);
        if (!string.IsNullOrWhiteSpace(request.Dto.BoundaryWkt))
            return boundaryParser.ParseWkt(request.Dto.BoundaryWkt);
        return null;
    }
}
