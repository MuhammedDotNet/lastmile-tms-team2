using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Vehicles.Reads;

public interface IVehicleReadService
{
    IQueryable<Vehicle> GetVehicles();
}
