using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Vehicles.Mappings;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Vehicles.Commands;

public sealed class CreateVehicleCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<CreateVehicleCommand, Vehicle>
{
    public async Task<Vehicle> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var plateExists = await dbContext.Vehicles
            .AnyAsync(v => v.RegistrationPlate == request.Dto.RegistrationPlate, cancellationToken);
        if (plateExists)
            throw new InvalidOperationException($"Vehicle with registration plate '{request.Dto.RegistrationPlate}' already exists.");

        var now = DateTimeOffset.UtcNow;
        var vehicle = request.Dto.ToEntity();
        vehicle.CreatedAt = now;
        vehicle.CreatedBy = currentUser.UserName ?? currentUser.UserId;

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

        return vehicle;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message?.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message?.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message?.Contains("23505", StringComparison.OrdinalIgnoreCase) == true;
    }
}
