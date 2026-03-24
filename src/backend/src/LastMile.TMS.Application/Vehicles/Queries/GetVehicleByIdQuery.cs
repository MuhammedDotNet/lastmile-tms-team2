using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Queries;

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDto?>;

public class GetVehicleByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetVehicleByIdQuery, VehicleDto?>
{
    public async Task<VehicleDto?> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .Include(v => v.Depot)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicle is null)
            return null;

        var totalRoutes = await dbContext.Routes
            .CountAsync(r => r.VehicleId == vehicle.Id, cancellationToken);

        var completedRoutes = await dbContext.Routes
            .Where(r => r.VehicleId == vehicle.Id && r.Status == RouteStatus.Completed)
            .ToListAsync(cancellationToken);

        var routesCompleted = completedRoutes.Count;
        var totalMileage = completedRoutes.Sum(r => r.TotalMileage);

        return new VehicleDto
        {
            Id = vehicle.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            ParcelCapacity = vehicle.ParcelCapacity,
            WeightCapacity = vehicle.WeightCapacity,
            Status = vehicle.Status,
            DepotId = vehicle.DepotId,
            DepotName = vehicle.Depot?.Name ?? string.Empty,
            TotalRoutes = totalRoutes,
            RoutesCompleted = routesCompleted,
            TotalMileage = totalMileage,
            CreatedAt = vehicle.CreatedAt,
            LastModifiedAt = vehicle.LastModifiedAt
        };
    }
}