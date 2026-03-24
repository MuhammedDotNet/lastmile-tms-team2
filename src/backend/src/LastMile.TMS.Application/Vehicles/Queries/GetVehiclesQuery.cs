using LastMile.TMS.Application.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Queries;

public record GetVehiclesQuery(
    int Page = 1,
    int PageSize = 20,
    VehicleStatus? Status = null,
    Guid? DepotId = null
) : IRequest<PaginatedResult<VehicleDto>>;

public class GetVehiclesQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetVehiclesQuery, PaginatedResult<VehicleDto>>
{
    public async Task<PaginatedResult<VehicleDto>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, PaginationDefaults.MaxPageSize);
        var page = Math.Max(1, request.Page);

        var query = dbContext.Vehicles.Include(v => v.Depot).AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(v => v.Status == request.Status.Value);
        }

        if (request.DepotId.HasValue)
        {
            query = query.Where(v => v.DepotId == request.DepotId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var vehicleIds = items.Select(v => v.Id).ToList();
        Dictionary<Guid, (int TotalRoutes, int RoutesCompleted, int TotalMileage)> routeStats = new();
        if (vehicleIds.Count > 0)
        {
            var aggregates = await dbContext.Routes
                .AsNoTracking()
                .Where(r => vehicleIds.Contains(r.VehicleId))
                .GroupBy(r => r.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalRoutes = g.Count(),
                    RoutesCompleted = g.Count(r => r.Status == RouteStatus.Completed),
                    TotalMileage = g.Sum(r =>
                        r.Status == RouteStatus.Completed && r.EndMileage > 0
                            ? r.EndMileage - r.StartMileage
                            : 0)
                })
                .ToListAsync(cancellationToken);

            routeStats = aggregates.ToDictionary(
                x => x.VehicleId,
                x => (x.TotalRoutes, x.RoutesCompleted, x.TotalMileage));
        }

        return new PaginatedResult<VehicleDto>
        {
            Items = [.. items.Select(v =>
            {
                var hasStats = routeStats.TryGetValue(v.Id, out var stats);
                return new VehicleDto
                {
                    Id = v.Id,
                    RegistrationPlate = v.RegistrationPlate,
                    Type = v.Type,
                    ParcelCapacity = v.ParcelCapacity,
                    WeightCapacity = v.WeightCapacity,
                    Status = v.Status,
                    DepotId = v.DepotId,
                    DepotName = v.Depot?.Name ?? string.Empty,
                    TotalRoutes = hasStats ? stats.TotalRoutes : 0,
                    RoutesCompleted = hasStats ? stats.RoutesCompleted : 0,
                    TotalMileage = hasStats ? stats.TotalMileage : 0,
                    CreatedAt = v.CreatedAt,
                    LastModifiedAt = v.LastModifiedAt
                };
            })],
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
