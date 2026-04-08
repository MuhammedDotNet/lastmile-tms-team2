using FluentAssertions;
using LastMile.TMS.Application.BinLocations.Commands;
using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Queries;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Application.Tests.BinLocations;

public class BinLocationCommandHandlerTests
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static AppDbContext MakeDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    private static async Task<Depot> SeedDepotAsync(AppDbContext db, string name = "North Depot")
    {
        var address = new Address
        {
            Street1 = $"{name} Street",
            City = "Melbourne",
            State = "VIC",
            PostalCode = "3000",
            CountryCode = "AU",
        };

        var depot = new Depot
        {
            Name = name,
            Address = address,
            IsActive = true,
        };

        db.Depots.Add(depot);
        await db.SaveChangesAsync();
        return depot;
    }

    private static async Task<StorageZone> SeedStorageZoneAsync(
        AppDbContext db,
        Depot? depot = null,
        string name = "Storage Zone A")
    {
        depot ??= await SeedDepotAsync(db);

        var storageZone = new StorageZone
        {
            Name = name,
            NormalizedName = Normalize(name),
            DepotId = depot.Id,
            Depot = depot,
        };

        db.StorageZones.Add(storageZone);
        await db.SaveChangesAsync();
        return storageZone;
    }

    private static async Task<StorageAisle> SeedStorageAisleAsync(
        AppDbContext db,
        StorageZone? storageZone = null,
        string name = "Aisle A")
    {
        storageZone ??= await SeedStorageZoneAsync(db);

        var aisle = new StorageAisle
        {
            Name = name,
            NormalizedName = Normalize(name),
            StorageZoneId = storageZone.Id,
            StorageZone = storageZone,
        };

        db.StorageAisles.Add(aisle);
        await db.SaveChangesAsync();
        return aisle;
    }

    private static async Task<BinLocation> SeedBinLocationAsync(
        AppDbContext db,
        StorageAisle? storageAisle = null,
        string name = "BIN-01",
        bool isActive = true,
        Zone? deliveryZone = null)
    {
        storageAisle ??= await SeedStorageAisleAsync(db);

        var bin = new BinLocation
        {
            Name = name,
            NormalizedName = Normalize(name),
            StorageAisleId = storageAisle.Id,
            StorageAisle = storageAisle,
            IsActive = isActive,
            DeliveryZoneId = deliveryZone?.Id,
            DeliveryZone = deliveryZone,
        };

        db.BinLocations.Add(bin);
        await db.SaveChangesAsync();
        return bin;
    }

    private static async Task<Zone> SeedDeliveryZoneAsync(
        AppDbContext db,
        Depot? depot = null,
        string name = "Delivery Zone A",
        bool isActive = true)
    {
        depot ??= await SeedDepotAsync(db);

        var zone = new Zone
        {
            Name = name,
            Boundary = CreateBoundary(),
            IsActive = isActive,
            DepotId = depot.Id,
            Depot = depot,
        };

        db.Zones.Add(zone);
        await db.SaveChangesAsync();
        return zone;
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

    [Fact]
    public async Task CreateStorageZone_WithDuplicateNameInSameDepot_IgnoringCase_Throws()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db);
        await SeedStorageZoneAsync(db, depot, "Alpha");

        var handler = new CreateStorageZoneCommandHandler(db);
        var command = new CreateStorageZoneCommand(new CreateStorageZoneDto
        {
            DepotId = depot.Id,
            Name = " alpha "
        });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task UpdateBinLocation_UpdatesTrimmedNameAndActiveState()
    {
        var db = MakeDbContext();
        var bin = await SeedBinLocationAsync(db, name: "BIN-01", isActive: true);
        var handler = new UpdateBinLocationCommandHandler(db);

        var result = await handler.Handle(
            new UpdateBinLocationCommand(
                bin.Id,
                new UpdateBinLocationDto
                {
                    Name = "  BIN-02  ",
                    IsActive = false
                }),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("BIN-02");
        result.IsActive.Should().BeFalse();

        var persisted = await db.BinLocations.SingleAsync(x => x.Id == bin.Id);
        persisted.Name.Should().Be("BIN-02");
        persisted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBinLocation_WithAssignedDeliveryZone_ReturnsAssignment()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db);
        var aisle = await SeedStorageAisleAsync(db, await SeedStorageZoneAsync(db, depot));
        var deliveryZone = await SeedDeliveryZoneAsync(db, depot, "North Metro");
        var handler = new CreateBinLocationCommandHandler(db);

        var result = await handler.Handle(
            new CreateBinLocationCommand(new CreateBinLocationDto
            {
                StorageAisleId = aisle.Id,
                Name = "BIN-10",
                IsActive = true,
                DeliveryZoneId = deliveryZone.Id,
            }),
            CancellationToken.None);

        result.DeliveryZoneId.Should().Be(deliveryZone.Id);
        result.DeliveryZoneName.Should().Be("North Metro");

        var persisted = await db.BinLocations.SingleAsync(x => x.Id == result.Id);
        persisted.DeliveryZoneId.Should().Be(deliveryZone.Id);
    }

    [Fact]
    public async Task CreateBinLocation_WithDeliveryZoneFromDifferentDepot_Throws()
    {
        var db = MakeDbContext();
        var depotA = await SeedDepotAsync(db, "Depot A");
        var depotB = await SeedDepotAsync(db, "Depot B");
        var aisle = await SeedStorageAisleAsync(db, await SeedStorageZoneAsync(db, depotA));
        var deliveryZone = await SeedDeliveryZoneAsync(db, depotB, "South Metro");
        var handler = new CreateBinLocationCommandHandler(db);

        var act = () => handler.Handle(
            new CreateBinLocationCommand(new CreateBinLocationDto
            {
                StorageAisleId = aisle.Id,
                Name = "BIN-10",
                IsActive = true,
                DeliveryZoneId = deliveryZone.Id,
            }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*same depot*");
    }

    [Fact]
    public async Task CreateBinLocation_WithTakenDeliveryZoneAndActiveBin_Throws()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db);
        var storageZone = await SeedStorageZoneAsync(db, depot);
        var firstAisle = await SeedStorageAisleAsync(db, storageZone, "Aisle A");
        var secondAisle = await SeedStorageAisleAsync(db, storageZone, "Aisle B");
        var deliveryZone = await SeedDeliveryZoneAsync(db, depot, "North Metro");
        await SeedBinLocationAsync(db, firstAisle, "BIN-01", true, deliveryZone);
        var handler = new CreateBinLocationCommandHandler(db);

        var act = () => handler.Handle(
            new CreateBinLocationCommand(new CreateBinLocationDto
            {
                StorageAisleId = secondAisle.Id,
                Name = "BIN-02",
                IsActive = true,
                DeliveryZoneId = deliveryZone.Id,
            }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already assigned*");
    }

    [Fact]
    public async Task CreateBinLocation_WithTakenDeliveryZoneAndInactiveBin_AllowsReuse()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db);
        var storageZone = await SeedStorageZoneAsync(db, depot);
        var firstAisle = await SeedStorageAisleAsync(db, storageZone, "Aisle A");
        var secondAisle = await SeedStorageAisleAsync(db, storageZone, "Aisle B");
        var deliveryZone = await SeedDeliveryZoneAsync(db, depot, "North Metro");
        await SeedBinLocationAsync(db, firstAisle, "BIN-01", true, deliveryZone);
        var handler = new CreateBinLocationCommandHandler(db);

        var result = await handler.Handle(
            new CreateBinLocationCommand(new CreateBinLocationDto
            {
                StorageAisleId = secondAisle.Id,
                Name = "BIN-02",
                IsActive = false,
                DeliveryZoneId = deliveryZone.Id,
            }),
            CancellationToken.None);

        result.IsActive.Should().BeFalse();
        result.DeliveryZoneId.Should().Be(deliveryZone.Id);
    }

    [Fact]
    public async Task UpdateBinLocation_WithNullIsActive_KeepsCurrentStatus()
    {
        var db = MakeDbContext();
        var bin = await SeedBinLocationAsync(db, name: "BIN-01", isActive: true);
        var handler = new UpdateBinLocationCommandHandler(db);

        var result = await handler.Handle(
            new UpdateBinLocationCommand(
                bin.Id,
                new UpdateBinLocationDto
                {
                    Name = "BIN-02",
                    IsActive = null
                }),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();

        var persisted = await db.BinLocations.SingleAsync(x => x.Id == bin.Id);
        persisted.Name.Should().Be("BIN-02");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateBinLocation_AssignsAndClearsDeliveryZone()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db);
        var aisle = await SeedStorageAisleAsync(db, await SeedStorageZoneAsync(db, depot));
        var deliveryZone = await SeedDeliveryZoneAsync(db, depot, "North Metro");
        var bin = await SeedBinLocationAsync(db, aisle, "BIN-01", true);
        var handler = new UpdateBinLocationCommandHandler(db);

        var assigned = await handler.Handle(
            new UpdateBinLocationCommand(
                bin.Id,
                new UpdateBinLocationDto
                {
                    Name = "BIN-01",
                    DeliveryZoneIdSpecified = true,
                    DeliveryZoneId = deliveryZone.Id,
                }),
            CancellationToken.None);

        assigned.Should().NotBeNull();
        assigned!.DeliveryZoneId.Should().Be(deliveryZone.Id);
        assigned.DeliveryZoneName.Should().Be("North Metro");

        var cleared = await handler.Handle(
            new UpdateBinLocationCommand(
                bin.Id,
                new UpdateBinLocationDto
                {
                    Name = "BIN-01",
                    DeliveryZoneIdSpecified = true,
                    DeliveryZoneId = null,
                }),
            CancellationToken.None);

        cleared.Should().NotBeNull();
        cleared!.DeliveryZoneId.Should().BeNull();
        cleared.DeliveryZoneName.Should().BeNull();

        var persisted = await db.BinLocations.SingleAsync(x => x.Id == bin.Id);
        persisted.DeliveryZoneId.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStorageZone_WithExistingAisles_Throws()
    {
        var db = MakeDbContext();
        var storageZone = await SeedStorageZoneAsync(db);
        await SeedStorageAisleAsync(db, storageZone);
        var handler = new DeleteStorageZoneCommandHandler(db);

        var act = () => handler.Handle(new DeleteStorageZoneCommand(storageZone.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*aisle*");
    }

    [Fact]
    public async Task DeleteStorageAisle_WithExistingBins_Throws()
    {
        var db = MakeDbContext();
        var aisle = await SeedStorageAisleAsync(db);
        await SeedBinLocationAsync(db, aisle);
        var handler = new DeleteStorageAisleCommandHandler(db);

        var act = () => handler.Handle(new DeleteStorageAisleCommand(aisle.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*bin*");
    }

    [Fact]
    public async Task GetDepotStorageLayout_ReturnsHierarchyOrderedByName()
    {
        var db = MakeDbContext();
        var depot = await SeedDepotAsync(db, "South Depot");
        var deliveryZoneB = await SeedDeliveryZoneAsync(db, depot, "Zone Delivery B");
        var deliveryZoneA = await SeedDeliveryZoneAsync(db, depot, "Zone Delivery A");

        var zoneB = await SeedStorageZoneAsync(db, depot, "Zone B");
        var zoneA = await SeedStorageZoneAsync(db, depot, "Zone A");

        var aisleB = await SeedStorageAisleAsync(db, zoneA, "Aisle B");
        var aisleA = await SeedStorageAisleAsync(db, zoneA, "Aisle A");

        await SeedBinLocationAsync(db, aisleA, "BIN-02", false, deliveryZoneB);
        await SeedBinLocationAsync(db, aisleA, "BIN-01", true, deliveryZoneA);
        await SeedStorageAisleAsync(db, zoneB, "Aisle C");

        db.ChangeTracker.Clear();

        var handler = new GetDepotStorageLayoutQueryHandler(db);

        var result = await handler.Handle(new GetDepotStorageLayoutQuery(depot.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.DepotId.Should().Be(depot.Id);
        result.DepotName.Should().Be("South Depot");
        result.AvailableDeliveryZones.Select(x => x.Name).Should().Equal("Zone Delivery A", "Zone Delivery B");
        result.StorageZones.Select(x => x.Name).Should().Equal("Zone A", "Zone B");
        result.StorageZones[0].StorageAisles.Select(x => x.Name).Should().Equal("Aisle A", "Aisle B");
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.Name).Should().Equal("BIN-01", "BIN-02");
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.IsActive).Should().Equal(true, false);
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.DeliveryZoneName).Should().Equal("Zone Delivery A", "Zone Delivery B");
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.DeliveryZoneId).Should().Equal(deliveryZoneA.Id, deliveryZoneB.Id);
    }
}
