using LastMile.TMS.Application.Parcels.Services;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Infrastructure.Services;

/// <summary>
/// Deterministic geocoding used when test support is enabled so end-to-end flows
/// do not depend on external HTTP geocoding availability.
/// </summary>
public sealed class TestSupportGeocodingService : IGeocodingService
{
    private static readonly Point DeterministicPoint =
        NtsGeometryServices.Instance
            .CreateGeometryFactory(srid: 4326)
            .CreatePoint(new Coordinate(151.5, -33.5));

    public Task<Point?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Point?>(DeterministicPoint);
    }
}
