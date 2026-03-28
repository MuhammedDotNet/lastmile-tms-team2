using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Zones.Mappings;
using LastMile.TMS.Application.Zones.Services;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Zones.Commands;

public sealed class CreateZoneCommandHandler(
    IAppDbContext db,
    IZoneBoundaryParser boundaryParser)
    : IRequestHandler<CreateZoneCommand, Zone>
{
    public async Task<Zone> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
    {
        var polygon = ParseBoundary(request);
        if (polygon is null)
            throw new ArgumentException("Failed to parse zone boundary from the provided input.");

        var zone = request.Dto.ToEntity();
        zone.Boundary = polygon;

        db.Zones.Add(zone);
        await db.SaveChangesAsync(cancellationToken);
        return zone;
    }

    private NetTopologySuite.Geometries.Polygon? ParseBoundary(CreateZoneCommand request)
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
