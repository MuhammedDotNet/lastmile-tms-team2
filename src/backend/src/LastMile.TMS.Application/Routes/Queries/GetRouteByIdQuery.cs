using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Routes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Queries;

public record GetRouteByIdQuery(Guid Id) : IRequest<RouteDto?>;

public class GetRouteByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetRouteByIdQuery, RouteDto?>
{
    public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
    {
        var route = await dbContext.Routes
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
                .ThenInclude(d => d.User)
            .Include(r => r.Parcels)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route is null)
            return null;

        return route.ToDto();
    }
}
