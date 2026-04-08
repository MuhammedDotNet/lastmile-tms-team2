using FluentAssertions;
using LastMile.TMS.Application.Zones.Commands;
using LastMile.TMS.Application.Zones.DTOs;
using LastMile.TMS.Application.Zones.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Application.Tests.Zones;

public class ZoneCommandHandlerTests
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private static AppDbContext MakeDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    [Fact]
    public async Task DeleteZone_WithAssignedBinLocations_Throws()
    {
        var db = MakeDbContext();
        var zone = await SeedAssignedZoneAsync(db);
        var handler = new DeleteZoneCommandHandler(db);

        var act = () => handler.Handle(new DeleteZoneCommand(zone.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*assigned*bin*");
    }

    [Fact]
    public async Task UpdateZone_DeactivatingAssignedZone_Throws()
    {
        var db = MakeDbContext();
        var zone = await SeedAssignedZoneAsync(db);
        var handler = new UpdateZoneCommandHandler(db, new StubZoneBoundaryParser());

        var act = () => handler.Handle(
            new UpdateZoneCommand(
                zone.Id,
                new UpdateZoneDto
                {
                    Name = zone.Name,
                    DepotId = zone.DepotId,
                    IsActive = false,
                }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*assigned*bin*");
    }

    [Fact]
    public async Task UpdateZone_MovingAssignedZoneToDifferentDepot_Throws()
    {
        var db = MakeDbContext();
        var zone = await SeedAssignedZoneAsync(db);
        var otherDepot = await SeedDepotAsync(db, "South Depot");
        var handler = new UpdateZoneCommandHandler(db, new StubZoneBoundaryParser());

        var act = () => handler.Handle(
            new UpdateZoneCommand(
                zone.Id,
                new UpdateZoneDto
                {
                    Name = zone.Name,
                    DepotId = otherDepot.Id,
                    IsActive = true,
                }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*different depot*");
    }

    private static async Task<Zone> SeedAssignedZoneAsync(AppDbContext db)
    {
        var depot = await SeedDepotAsync(db, "North Depot");
        var storageZone = new StorageZone
        {
            Name = "Storage Zone A",
            NormalizedName = "STORAGE ZONE A",
            DepotId = depot.Id,
            Depot = depot,
        };
        var aisle = new StorageAisle
        {
            Name = "Aisle A",
            NormalizedName = "AISLE A",
            StorageZone = storageZone,
        };
        var deliveryZone = new Zone
        {
            Name = "Metro North",
            Boundary = CreateBoundary(),
            IsActive = true,
            DepotId = depot.Id,
            Depot = depot,
        };
        var bin = new BinLocation
        {
            Name = "BIN-01",
            NormalizedName = "BIN-01",
            IsActive = true,
            StorageAisle = aisle,
            DeliveryZone = deliveryZone,
        };

        db.StorageZones.Add(storageZone);
        db.StorageAisles.Add(aisle);
        db.Zones.Add(deliveryZone);
        db.BinLocations.Add(bin);
        await db.SaveChangesAsync();

        return deliveryZone;
    }

    private static async Task<Depot> SeedDepotAsync(AppDbContext db, string name)
    {
        var depot = new Depot
        {
            Name = name,
            Address = new Address
            {
                Street1 = $"{name} Street",
                City = "Melbourne",
                State = "VIC",
                PostalCode = "3000",
                CountryCode = "AU",
            },
            IsActive = true,
        };

        db.Depots.Add(depot);
        await db.SaveChangesAsync();
        return depot;
    }

    private static Polygon CreateBoundary()
    {
        var polygon = GeometryFactory.CreatePolygon(
            [
                new Coordinate(144.95, -37.82),
                new Coordinate(144.98, -37.82),
                new Coordinate(144.98, -37.79),
                new Coordinate(144.95, -37.79),
                new Coordinate(144.95, -37.82),
            ]);
        polygon.SRID = 4326;
        return polygon;
    }

    private sealed class StubZoneBoundaryParser : IZoneBoundaryParser
    {
        public Polygon? ParseGeoJson(string geoJson) => null;

        public Polygon? ParseCoordinates(List<List<double>> coordinates) => null;

        public Polygon? ParseWkt(string wkt) => null;
    }
}
