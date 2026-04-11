using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Infrastructure.Services;

/// <summary>
/// Finds the most specific active zone whose boundary contains the given point.
/// Uses the Npgsql NetTopologySuite provider to translate Boundary.Covers(point)
/// to PostGIS ST_Covers so the GIST spatial index on Zone.Boundary is used.
/// When multiple active zones overlap, the smallest boundary wins to avoid a
/// broad fallback zone swallowing a more precise delivery zone.
/// </summary>
public class ZoneMatchingService : IZoneMatchingService
{
    private readonly IAppDbContext _db;

    public ZoneMatchingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> FindZoneIdAsync(Point point, CancellationToken cancellationToken = default)
    {
        if (point is null)
            return null;

        var matchingZones = await _db.Zones
            .AsNoTracking()
            .Where(z => z.IsActive && z.Boundary.Covers(point))
            .Select(z => new ZoneMatchCandidate(z.Id, z.Boundary.Area, z.CreatedAt))
            .ToListAsync(cancellationToken);

        return matchingZones
            .OrderBy(candidate => candidate.Area)
            .ThenByDescending(candidate => candidate.CreatedAt)
            .ThenBy(candidate => candidate.Id)
            .Select(candidate => (Guid?)candidate.Id)
            .FirstOrDefault();
    }

    private sealed record ZoneMatchCandidate(Guid Id, double Area, DateTimeOffset CreatedAt);
}
