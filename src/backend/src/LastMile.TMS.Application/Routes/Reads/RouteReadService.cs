using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Reads;

public sealed class RouteReadService(IAppDbContext dbContext) : IRouteReadService
{
    public IQueryable<Route> GetRoutes() =>
        dbContext.Routes
            .AsNoTracking();
}
