using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Reads;

public sealed class DriverReadService(IAppDbContext dbContext) : IDriverReadService
{
    public IQueryable<Driver> GetDrivers(Guid? depotId = null)
    {
        var query = dbContext.Drivers
            .AsNoTracking()
            .Where(d => d.Status == DriverStatus.Active);

        if (depotId.HasValue)
        {
            query = query.Where(d => d.DepotId == depotId.Value);
        }

        return query
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName);
    }
}
