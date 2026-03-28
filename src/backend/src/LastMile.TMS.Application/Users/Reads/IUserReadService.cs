using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Users.Reads;

public interface IUserReadService
{
    IQueryable<ApplicationUser> GetUsers(
        string? search = null,
        bool? isActive = null,
        Guid? depotId = null,
        Guid? zoneId = null);
}
