using FluentAssertions;
using LastMile.TMS.Application.Drivers.Reads;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Tests.Drivers;

public class DriverReadServiceTests
{
    private static AppDbContext MakeDbContext()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);
    }

    private static Driver MakeDriver(
        string firstName,
        string lastName,
        DriverStatus status = DriverStatus.Active,
        Guid? depotId = null)
    {
        depotId ??= Guid.NewGuid();
        return new Driver
        {
            FirstName = firstName,
            LastName = lastName,
            LicenseNumber = $"LIC-{Guid.NewGuid():N}",
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
            ZoneId = Guid.NewGuid(),
            DepotId = depotId.Value,
            UserId = Guid.NewGuid(),
            Status = status,
        };
    }

    [Fact]
    public async Task GetDrivers_ReturnsOnlyActiveDrivers()
    {
        var db = MakeDbContext();
        var depotId = Guid.NewGuid();

        db.Drivers.AddRange(
            MakeDriver("Ali", "Ahmed", DriverStatus.Active, depotId),
            MakeDriver("Sara", "Mohamed", DriverStatus.Inactive, depotId),
            MakeDriver("Omar", "Hassan", DriverStatus.Active, depotId));
        await db.SaveChangesAsync();

        var service = new DriverReadService(db);
        var result = await service.GetDrivers().ToListAsync();

        result.Should().HaveCount(2);
        result.Select(d => d.FirstName).Should().Contain("Ali", "Omar")
            .And.NotContain("Sara");
    }

    [Fact]
    public async Task GetDrivers_WithDepotId_FiltersByDepot()
    {
        var db = MakeDbContext();
        var depot1 = Guid.NewGuid();
        var depot2 = Guid.NewGuid();

        db.Drivers.AddRange(
            MakeDriver("Ali", "Ahmed", DriverStatus.Active, depot1),
            MakeDriver("Sara", "Mohamed", DriverStatus.Active, depot2));
        await db.SaveChangesAsync();

        var service = new DriverReadService(db);
        var result = await service.GetDrivers(depot1).ToListAsync();

        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("Ali");
    }

    [Fact]
    public async Task GetDrivers_ReturnsEntityNameFields()
    {
        var db = MakeDbContext();

        db.Drivers.Add(MakeDriver("Ali", "Ahmed"));
        await db.SaveChangesAsync();

        var service = new DriverReadService(db);
        var result = await service.GetDrivers().ToListAsync();

        result[0].FirstName.Should().Be("Ali");
        result[0].LastName.Should().Be("Ahmed");
    }

    [Fact]
    public async Task GetDrivers_OrdersByLastNameThenFirstName()
    {
        var db = MakeDbContext();
        var depotId = Guid.NewGuid();

        db.Drivers.AddRange(
            MakeDriver("Sara", "Mohamed", DriverStatus.Active, depotId),
            MakeDriver("Ali", "Ahmed", DriverStatus.Active, depotId),
            MakeDriver("Omar", "Ahmed", DriverStatus.Active, depotId));
        await db.SaveChangesAsync();

        var service = new DriverReadService(db);
        var result = await service.GetDrivers().ToListAsync();

        result.Select(d => $"{d.FirstName} {d.LastName}").Should().Equal(
            "Ali Ahmed", "Omar Ahmed", "Sara Mohamed");
    }
}
