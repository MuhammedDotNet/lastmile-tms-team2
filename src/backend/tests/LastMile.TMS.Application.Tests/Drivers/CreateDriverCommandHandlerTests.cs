using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.Commands;
using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Drivers;

public class CreateDriverCommandHandlerTests
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);

    private static AppDbContext MakeDbContext()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);
    }

    private static List<CreateDriverAvailabilityDto> WeekAvailability()
    {
        return Enum.GetValues<DayOfWeek>()
            .Select(dow => new CreateDriverAvailabilityDto
            {
                DayOfWeek = dow,
                ShiftStart = new TimeOnly(8, 0),
                ShiftEnd = new TimeOnly(17, 0),
                IsAvailable = true,
            })
            .ToList();
    }

    private static async Task<(AppDbContext db, Guid zoneId, Guid depotId, Guid user1Id, Guid user2Id)>
        SeedAsync()
    {
        var db = MakeDbContext();

        var depotAddress = new Address
        {
            Street1 = "Depot St",
            City = "Cairo",
            State = "Cairo",
            PostalCode = "11511",
            CountryCode = "EG",
        };
        db.Addresses.Add(depotAddress);

        var depot = new Depot
        {
            Name = "Central",
            AddressId = depotAddress.Id,
            Address = depotAddress,
            IsActive = true,
        };
        db.Depots.Add(depot);

        var ring = GeoFactory.CreateLinearRing(new[]
        {
            new Coordinate(31.20, 29.90),
            new Coordinate(31.30, 29.90),
            new Coordinate(31.30, 30.00),
            new Coordinate(31.20, 30.00),
            new Coordinate(31.20, 29.90),
        });
        ring.SRID = 4326;
        var zonePolygon = GeoFactory.CreatePolygon(ring);

        var zone = new Zone
        {
            Name = "Maadi",
            Boundary = zonePolygon,
            IsActive = true,
            DepotId = depot.Id,
            Depot = depot,
        };
        db.Zones.Add(zone);

        var user1 = new ApplicationUser
        {
            UserName = "d1@test.com",
            Email = "d1@test.com",
            FirstName = "One",
            LastName = "Driver",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var user2 = new ApplicationUser
        {
            UserName = "d2@test.com",
            Email = "d2@test.com",
            FirstName = "Two",
            LastName = "Driver",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        return (db, zone.Id, depot.Id, user1.Id, user2.Id);
    }

    /// <summary>
    /// GraphQL integration tests create a driver with an empty availability list; this verifies
    /// weekly <see cref="DriverAvailability"/> rows are persisted when the schedule is non-empty.
    /// </summary>
    [Fact]
    public async Task Handle_OnSuccess_PersistsDriverAndAvailability()
    {
        var (db, zoneId, depotId, user1Id, _) = await SeedAsync();
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserName.Returns("tester");
        var handler = new CreateDriverCommandHandler(db, currentUser);

        var dto = new CreateDriverDto
        {
            FirstName = "Ali",
            LastName = "Ahmed",
            LicenseNumber = $"LIC-{Guid.NewGuid():N}",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = user1Id,
            Status = DriverStatus.Active,
            AvailabilitySchedule = WeekAvailability(),
        };

        var result = await handler.Handle(new CreateDriverCommand(dto), CancellationToken.None);

        result.FirstName.Should().Be("Ali");
        result.LastName.Should().Be("Ahmed");
        (await db.DriverAvailabilities.CountAsync(a => a.DriverId == result.Id)).Should().Be(7);
    }

    [Fact]
    public async Task Handle_ZoneNotFound_Throws()
    {
        var (db, _, depotId, user1Id, _) = await SeedAsync();
        var currentUser = Substitute.For<ICurrentUserService>();
        var handler = new CreateDriverCommandHandler(db, currentUser);

        var dto = new CreateDriverDto
        {
            FirstName = "A",
            LastName = "B",
            LicenseNumber = $"LIC-{Guid.NewGuid():N}",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = Guid.NewGuid(),
            DepotId = depotId,
            UserId = user1Id,
            AvailabilitySchedule = WeekAvailability(),
        };

        var act = () => handler.Handle(new CreateDriverCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Zone not found*");
    }

    [Fact]
    public async Task Handle_DuplicateLicense_Throws()
    {
        var (db, zoneId, depotId, user1Id, user2Id) = await SeedAsync();
        var currentUser = Substitute.For<ICurrentUserService>();
        var handler = new CreateDriverCommandHandler(db, currentUser);

        const string license = "LIC-DUP-001";
        var dto1 = new CreateDriverDto
        {
            FirstName = "A",
            LastName = "One",
            LicenseNumber = license,
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = user1Id,
            AvailabilitySchedule = WeekAvailability(),
        };
        await handler.Handle(new CreateDriverCommand(dto1), CancellationToken.None);

        var dto2 = new CreateDriverDto
        {
            FirstName = "B",
            LastName = "Two",
            LicenseNumber = license,
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = user2Id,
            AvailabilitySchedule = WeekAvailability(),
        };

        var act = () => handler.Handle(new CreateDriverCommand(dto2), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*license number already exists*");
    }

    [Fact]
    public async Task Handle_UserAlreadyLinked_Throws()
    {
        var (db, zoneId, depotId, user1Id, _) = await SeedAsync();
        var currentUser = Substitute.For<ICurrentUserService>();
        var handler = new CreateDriverCommandHandler(db, currentUser);

        var dto1 = new CreateDriverDto
        {
            FirstName = "A",
            LastName = "One",
            LicenseNumber = $"LIC-{Guid.NewGuid():N}",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = user1Id,
            AvailabilitySchedule = WeekAvailability(),
        };
        await handler.Handle(new CreateDriverCommand(dto1), CancellationToken.None);

        var dto2 = new CreateDriverDto
        {
            FirstName = "B",
            LastName = "Two",
            LicenseNumber = $"LIC-{Guid.NewGuid():N}",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = zoneId,
            DepotId = depotId,
            UserId = user1Id,
            AvailabilitySchedule = WeekAvailability(),
        };

        var act = () => handler.Handle(new CreateDriverCommand(dto2), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already linked to this user account*");
    }
}
