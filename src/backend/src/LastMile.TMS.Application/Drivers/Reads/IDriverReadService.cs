using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Drivers.Reads;

public interface IDriverReadService
{
    IQueryable<Driver> GetDrivers(Guid? depotId = null);
}
