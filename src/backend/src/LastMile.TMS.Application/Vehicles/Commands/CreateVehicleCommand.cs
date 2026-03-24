using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Commands;

public record CreateVehicleCommand(CreateVehicleDto Dto) : IRequest<VehicleDto>;

public class CreateVehicleCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var plateExists = await dbContext.Vehicles
            .AnyAsync(v => v.RegistrationPlate == request.Dto.RegistrationPlate, cancellationToken);
        if (plateExists)
            throw new InvalidOperationException($"Vehicle with registration plate '{request.Dto.RegistrationPlate}' already exists.");

        var now = DateTimeOffset.UtcNow;
        var vehicle = new Vehicle
        {
            RegistrationPlate = request.Dto.RegistrationPlate,
            Type = request.Dto.Type,
            ParcelCapacity = request.Dto.ParcelCapacity,
            WeightCapacity = request.Dto.WeightCapacity,
            Status = request.Dto.Status,
            DepotId = request.Dto.DepotId,
            CreatedAt = now,
            CreatedBy = currentUser.UserName ?? currentUser.UserId,
        };

        dbContext.Vehicles.Add(vehicle);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException(
                $"Vehicle with registration plate '{request.Dto.RegistrationPlate}' already exists.");
        }

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
