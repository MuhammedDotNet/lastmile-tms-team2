using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Application.Drivers.Mappings;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Commands;

public sealed class UpdateDriverCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser,
    IDriverPhotoFileCleanup driverPhotoFileCleanup) : IRequestHandler<UpdateDriverCommand, Driver>
{
    public async Task<Driver> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await dbContext.Drivers
            .Include(d => d.AvailabilitySchedule)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (driver is null)
            throw new InvalidOperationException("Driver not found");

        var previousPhotoUrl = driver.PhotoUrl;

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

        var userLinkedToOtherDriver = await dbContext.Drivers
            .AnyAsync(d => d.UserId == request.Dto.UserId && d.Id != request.Id, cancellationToken);

        if (userLinkedToOtherDriver)
            throw new InvalidOperationException("A driver is already linked to this user account");

        var existingDriverWithLicense = await dbContext.Drivers
            .AnyAsync(d => d.LicenseNumber == request.Dto.LicenseNumber && d.Id != request.Id, cancellationToken);

        if (existingDriverWithLicense)
            throw new InvalidOperationException("A driver with this license number already exists");

        var now = DateTimeOffset.UtcNow;
        request.Dto.UpdateEntity(driver);
        driver.LastModifiedAt = now;
        driver.LastModifiedBy = currentUser.UserName ?? currentUser.UserId;

        var existingAvailability = driver.AvailabilitySchedule.ToList();
        var toRemove = existingAvailability
            .Where(ea => !request.Dto.AvailabilitySchedule.Any(ua => ua.Id == ea.Id))
            .ToList();

        foreach (var availability in toRemove)
        {
            dbContext.DriverAvailabilities.Remove(availability);
        }

        foreach (var availabilityDto in request.Dto.AvailabilitySchedule)
        {
            if (availabilityDto.Id.HasValue)
            {
                var existing = existingAvailability.FirstOrDefault(ea => ea.Id == availabilityDto.Id.Value);
                if (existing is not null)
                {
                    existing.DayOfWeek = availabilityDto.DayOfWeek;
                    existing.ShiftStart = availabilityDto.ShiftStart;
                    existing.ShiftEnd = availabilityDto.ShiftEnd;
                    existing.IsAvailable = availabilityDto.IsAvailable;
                    existing.LastModifiedAt = now;
                    existing.LastModifiedBy = currentUser.UserName ?? currentUser.UserId;
                }
            }
            else
            {
                var newAvailability = new DriverAvailability
                {
                    DriverId = driver.Id,
                    DayOfWeek = availabilityDto.DayOfWeek,
                    ShiftStart = availabilityDto.ShiftStart,
                    ShiftEnd = availabilityDto.ShiftEnd,
                    IsAvailable = availabilityDto.IsAvailable,
                    CreatedAt = now,
                    CreatedBy = currentUser.UserName ?? currentUser.UserId
                };
                dbContext.DriverAvailabilities.Add(newAvailability);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!string.Equals(previousPhotoUrl, driver.PhotoUrl, StringComparison.Ordinal))
            driverPhotoFileCleanup.TryDeleteStoredPhoto(previousPhotoUrl);

        driver.Zone = (await dbContext.Zones.FindAsync([driver.ZoneId], cancellationToken))!;
        driver.Depot = (await dbContext.Depots.FindAsync([driver.DepotId], cancellationToken))!;
        driver.User = (await dbContext.Users.FindAsync([driver.UserId], cancellationToken))!;

        return driver;
    }
}
