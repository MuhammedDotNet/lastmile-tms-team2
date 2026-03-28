using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Zones.Reads;

public interface IZoneReadService
{
    IQueryable<Zone> GetZones();
    Task<Zone?> GetZoneByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
