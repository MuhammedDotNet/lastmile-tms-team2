using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Reads;

public sealed class VehicleReadService(IAppDbContext dbContext) : IVehicleReadService
{
    public IQueryable<Vehicle> GetVehicles() =>
        dbContext.Vehicles
            .AsNoTracking();
}
