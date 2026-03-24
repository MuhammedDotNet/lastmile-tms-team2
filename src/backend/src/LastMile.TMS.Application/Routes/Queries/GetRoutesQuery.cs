using LastMile.TMS.Application.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Routes.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Queries;

public record GetRoutesQuery(
    Guid? VehicleId = null,
    RouteStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<RouteDto>>;

public class GetRoutesQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetRoutesQuery, PaginatedResult<RouteDto>>
{
    public async Task<PaginatedResult<RouteDto>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, PaginationDefaults.MaxPageSize);

        var query = dbContext.Routes
            .Include(r => r.Vehicle)
            .Include(r => r.Driver)
                .ThenInclude(d => d.User)
            .Include(r => r.Parcels)
            .AsQueryable();

        if (request.VehicleId.HasValue)
            query = query.Where(r => r.VehicleId == request.VehicleId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.StartDate)
            .Skip((Math.Max(1, request.Page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RouteDto>
        {
            Items = [.. items.Select(RouteMapping.ToDto)],
            TotalCount = totalCount,
            Page = Math.Max(1, request.Page),
            PageSize = pageSize
        };
    }
}
