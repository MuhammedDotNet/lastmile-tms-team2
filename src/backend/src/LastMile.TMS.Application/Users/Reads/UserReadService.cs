using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Users.Reads;

public sealed class UserReadService(IAppDbContext dbContext) : IUserReadService
{
    public IQueryable<ApplicationUser> GetUsers(
        string? search = null,
        bool? isActive = null,
        Guid? depotId = null,
        Guid? zoneId = null)
    {
        var query = dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim().ToUpperInvariant();
            query = query.Where(u =>
                (u.FirstName + " " + u.LastName).ToUpper().Contains(pattern) ||
                u.Email!.ToUpper().Contains(pattern) ||
                (u.PhoneNumber ?? string.Empty).ToUpper().Contains(pattern));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (depotId.HasValue)
        {
            query = query.Where(u => u.DepotId == depotId.Value);
        }

        if (zoneId.HasValue)
        {
            query = query.Where(u => u.ZoneId == zoneId.Value);
        }

        return query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName);
    }
}
