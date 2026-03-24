using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Commands;

public record UpdateVehicleCommand(Guid Id, UpdateVehicleDto Dto) : IRequest<VehicleDto?>;

public class UpdateVehicleCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<UpdateVehicleCommand, VehicleDto?>
{
    public async Task<VehicleDto?> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .Include(v => v.Depot)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicle is null)
            return null;

        if (vehicle.RegistrationPlate != request.Dto.RegistrationPlate)
        {
            var plateExists = await dbContext.Vehicles
                .AnyAsync(v => v.RegistrationPlate == request.Dto.RegistrationPlate && v.Id != request.Id, cancellationToken);
            if (plateExists)
                throw new InvalidOperationException($"Vehicle with registration plate '{request.Dto.RegistrationPlate}' already exists.");
        }

        if (request.Dto.Status == VehicleStatus.Available)
        {
            var hasActiveRoute = await dbContext.Routes
                .AnyAsync(
                    r => r.VehicleId == vehicle.Id
                        && (r.Status == RouteStatus.Planned || r.Status == RouteStatus.InProgress),
                    cancellationToken);

            if (hasActiveRoute)
                throw new InvalidOperationException(
                    "Cannot set vehicle to Available while it has a planned or in-progress route. Complete or cancel the routes first.");
        }

        var now = DateTimeOffset.UtcNow;
        vehicle.RegistrationPlate = request.Dto.RegistrationPlate;
        vehicle.Type = request.Dto.Type;
        vehicle.ParcelCapacity = request.Dto.ParcelCapacity;
        vehicle.WeightCapacity = request.Dto.WeightCapacity;
        vehicle.Status = request.Dto.Status;
        vehicle.DepotId = request.Dto.DepotId;
        vehicle.LastModifiedAt = now;
        vehicle.LastModifiedBy = currentUser.UserName ?? currentUser.UserId;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException(
                $"Vehicle with registration plate '{request.Dto.RegistrationPlate}' already exists.");
        }

        var routeStats = await dbContext.Routes
            .Where(r => r.VehicleId == vehicle.Id)
            .GroupBy(r => r.VehicleId)
            .Select(g => new
            {
                TotalRoutes = g.Count(),
                RoutesCompleted = g.Count(r => r.Status == RouteStatus.Completed),
                TotalMileage = g.Sum(r =>
                    r.Status == RouteStatus.Completed && r.EndMileage > 0
                        ? r.EndMileage - r.StartMileage
                        : 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var depot = await dbContext.Depots.FindAsync([vehicle.DepotId], cancellationToken);

        return new VehicleDto
        {
            Id = vehicle.Id,
            RegistrationPlate = vehicle.RegistrationPlate,
            Type = vehicle.Type,
            ParcelCapacity = vehicle.ParcelCapacity,
            WeightCapacity = vehicle.WeightCapacity,
            Status = vehicle.Status,
            DepotId = vehicle.DepotId,
            DepotName = depot?.Name ?? string.Empty,
            TotalRoutes = routeStats?.TotalRoutes ?? 0,
            RoutesCompleted = routeStats?.RoutesCompleted ?? 0,
            TotalMileage = routeStats?.TotalMileage ?? 0,
            CreatedAt = vehicle.CreatedAt,
            LastModifiedAt = vehicle.LastModifiedAt
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message?.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message?.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message?.Contains("23505", StringComparison.OrdinalIgnoreCase) == true;
    }
}
