using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Depot> Depots { get; }
    DbSet<Route> Routes { get; }
    DbSet<Parcel> Parcels { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
