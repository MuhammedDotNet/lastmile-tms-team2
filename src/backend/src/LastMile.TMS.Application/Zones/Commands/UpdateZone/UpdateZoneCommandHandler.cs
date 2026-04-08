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

        var assignedBinCount = (request.Dto.DepotId != zone.DepotId || (!request.Dto.IsActive && zone.IsActive))
            ? await db.BinLocations.CountAsync(x => x.DeliveryZoneId == zone.Id, cancellationToken)
            : 0;

        if (!request.Dto.IsActive && zone.IsActive && assignedBinCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot deactivate zone '{zone.Name}' because it is assigned to {assignedBinCount} bin location(s). Remove the bin assignments first.");
        }

        if (request.Dto.DepotId != zone.DepotId && assignedBinCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot move zone '{zone.Name}' to a different depot while it is assigned to bin locations.");
        }

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
