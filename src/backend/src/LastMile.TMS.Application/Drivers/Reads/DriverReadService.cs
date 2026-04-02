using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Reads;

public sealed class DriverReadService(IAppDbContext dbContext) : IDriverReadService
{
    public IQueryable<Driver> GetDrivers()
    {
        return dbContext.Drivers
            .AsNoTracking()
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName);
    }
}
