using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Routes.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Commands;

public record CreateRouteCommand(CreateRouteDto Dto) : IRequest<RouteDto>;

public class CreateRouteCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<CreateRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        // Get vehicle with capacity
        var vehicle = await dbContext.Vehicles
            .Include(v => v.Depot)
            .FirstOrDefaultAsync(v => v.Id == request.Dto.VehicleId, cancellationToken);

        if (vehicle is null)
            throw new InvalidOperationException("Vehicle not found");

        // Check if vehicle is available
        if (vehicle.Status != VehicleStatus.Available)
            throw new InvalidOperationException($"Vehicle is not available. Current status: {vehicle.Status}");

        // Get parcels to be assigned
        var parcels = await dbContext.Parcels
            .Where(p => request.Dto.ParcelIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (parcels.Count != request.Dto.ParcelIds.Count)
            throw new InvalidOperationException("One or more parcels not found");

        // CAPACITY ENFORCEMENT - Check parcel capacity
        var totalParcelCount = parcels.Count;
        if (totalParcelCount > vehicle.ParcelCapacity)
        {
            throw new InvalidOperationException(
                $"Parcel capacity exceeded. Vehicle capacity: {vehicle.ParcelCapacity}, Requested: {totalParcelCount}");
        }

        // CAPACITY ENFORCEMENT - Check weight capacity (Vehicle.WeightCapacity is in kg)
        var totalWeightKg = parcels.Sum(p => p.WeightUnit == WeightUnit.Lb ? p.Weight * 0.453592m : p.Weight);
        if (totalWeightKg > vehicle.WeightCapacity)
        {
            throw new InvalidOperationException(
                $"Weight capacity exceeded. Vehicle capacity: {vehicle.WeightCapacity}kg, Requested: {totalWeightKg:F2}kg");
        }

        // Get driver
        var driver = await dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.Dto.DriverId, cancellationToken);

        if (driver is null)
            throw new InvalidOperationException("Driver not found");

        // Prevent double-booking: same driver cannot have overlapping active routes.
        // CreateRouteDto has no end time; treat the new route as [StartDate, +∞). Must match Route.TimeRangesOverlap semantics.
        var requestedStart = request.Dto.StartDate;
        var requestedEndExclusive = DateTimeOffset.MaxValue;
        var driverHasOverlappingRoute = await dbContext.Routes
            .AsNoTracking()
            .AnyAsync(
                r => r.DriverId == request.Dto.DriverId
                    && (r.Status == RouteStatus.Planned || r.Status == RouteStatus.InProgress)
                    && r.StartDate < requestedEndExclusive
                    && requestedStart < (r.EndDate ?? DateTimeOffset.MaxValue),
                cancellationToken);

        if (driverHasOverlappingRoute)
            throw new InvalidOperationException(
                "Driver is already assigned to a planned or in-progress route that overlaps the requested time.");

        // Same overlap rule as for the driver: vehicle cannot serve two active routes in the same window.
        var vehicleHasOverlappingRoute = await dbContext.Routes
            .AsNoTracking()
            .AnyAsync(
                r => r.VehicleId == request.Dto.VehicleId
                    && (r.Status == RouteStatus.Planned || r.Status == RouteStatus.InProgress)
                    && r.StartDate < requestedEndExclusive
                    && requestedStart < (r.EndDate ?? DateTimeOffset.MaxValue),
                cancellationToken);

        if (vehicleHasOverlappingRoute)
            throw new InvalidOperationException(
                "Vehicle is already assigned to a planned or in-progress route that overlaps the requested time.");

        // Create route
        var now = DateTimeOffset.UtcNow;
        var route = new Route
        {
            VehicleId = request.Dto.VehicleId,
            DriverId = request.Dto.DriverId,
            StartDate = request.Dto.StartDate,
            StartMileage = request.Dto.StartMileage,
            Status = RouteStatus.Planned,
            CreatedAt = now,
            CreatedBy = currentUser.UserName ?? currentUser.UserId,
        };

        dbContext.Routes.Add(route);

        // Assign parcels to route and update status
        foreach (var parcel in parcels)
        {
            route.Parcels.Add(parcel);
            if (parcel.Status == ParcelStatus.Sorted || parcel.Status == ParcelStatus.Staged)
            {
                parcel.Status = ParcelStatus.Loaded;
            }
        }

        // Update vehicle status
        vehicle.Status = VehicleStatus.InUse;

        await dbContext.SaveChangesAsync(cancellationToken);

        route.Vehicle = vehicle;
        route.Driver = driver;
        return route.ToDto();
    }
}
