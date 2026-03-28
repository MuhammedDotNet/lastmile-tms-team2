using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Zones.Reads;

public sealed class ZoneReadService(IAppDbContext dbContext) : IZoneReadService
{
    public IQueryable<Zone> GetZones() =>
        dbContext.Zones
            .AsNoTracking();

    public Task<Zone?> GetZoneByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.Zones
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
}
