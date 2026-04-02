using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Application.Drivers.Mappings;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Commands;

public sealed class CreateDriverCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser) : IRequestHandler<CreateDriverCommand, Driver>
{
    public async Task<Driver> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        var zoneExists = await dbContext.Zones
            .AnyAsync(z => z.Id == request.Dto.ZoneId, cancellationToken);

        if (!zoneExists)
            throw new InvalidOperationException("Zone not found");

        var depotExists = await dbContext.Depots
            .AnyAsync(d => d.Id == request.Dto.DepotId, cancellationToken);

        if (!depotExists)
            throw new InvalidOperationException("Depot not found");

        var userExists = await dbContext.Users
            .AnyAsync(u => u.Id == request.Dto.UserId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException("User not found");

        var userAlreadyLinked = await dbContext.Drivers
            .AnyAsync(d => d.UserId == request.Dto.UserId, cancellationToken);

        if (userAlreadyLinked)
            throw new InvalidOperationException("A driver is already linked to this user account");

        var existingDriver = await dbContext.Drivers
            .AnyAsync(d => d.LicenseNumber == request.Dto.LicenseNumber, cancellationToken);

        if (existingDriver)
            throw new InvalidOperationException("A driver with this license number already exists");

        var now = DateTimeOffset.UtcNow;
        var driver = request.Dto.ToEntity();
        driver.CreatedAt = now;
        driver.CreatedBy = currentUser.UserName ?? currentUser.UserId;

        dbContext.Drivers.Add(driver);

        foreach (var availabilityDto in request.Dto.AvailabilitySchedule)
        {
            var availability = availabilityDto.ToEntity();
            availability.DriverId = driver.Id;
            availability.CreatedAt = now;
            availability.CreatedBy = currentUser.UserName ?? currentUser.UserId;
            dbContext.DriverAvailabilities.Add(availability);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        driver.Zone = (await dbContext.Zones.FindAsync([driver.ZoneId], cancellationToken))!;
        driver.Depot = (await dbContext.Depots.FindAsync([driver.DepotId], cancellationToken))!;
        driver.User = (await dbContext.Users.FindAsync([driver.UserId], cancellationToken))!;

        return driver;
    }
}
