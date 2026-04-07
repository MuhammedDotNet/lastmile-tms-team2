using FluentAssertions;
using LastMile.TMS.Application.BinLocations.Commands;
using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Queries;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Tests.BinLocations;

public class BinLocationCommandHandlerTests
{
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
        bool isActive = true)
    {
        storageAisle ??= await SeedStorageAisleAsync(db);

        var bin = new BinLocation
        {
            Name = name,
            StorageAisleId = storageAisle.Id,
            StorageAisle = storageAisle,
            IsActive = isActive,
        };

        db.BinLocations.Add(bin);
        await db.SaveChangesAsync();
        return bin;
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
                    StorageAisleId = bin.StorageAisleId,
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

        var zoneB = await SeedStorageZoneAsync(db, depot, "Zone B");
        var zoneA = await SeedStorageZoneAsync(db, depot, "Zone A");

        var aisleB = await SeedStorageAisleAsync(db, zoneA, "Aisle B");
        var aisleA = await SeedStorageAisleAsync(db, zoneA, "Aisle A");

        await SeedBinLocationAsync(db, aisleA, "BIN-02", false);
        await SeedBinLocationAsync(db, aisleA, "BIN-01", true);
        await SeedStorageAisleAsync(db, zoneB, "Aisle C");

        db.ChangeTracker.Clear();

        var handler = new GetDepotStorageLayoutQueryHandler(db);

        var result = await handler.Handle(new GetDepotStorageLayoutQuery(depot.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.DepotId.Should().Be(depot.Id);
        result.DepotName.Should().Be("South Depot");
        result.StorageZones.Select(x => x.Name).Should().Equal("Zone A", "Zone B");
        result.StorageZones[0].StorageAisles.Select(x => x.Name).Should().Equal("Aisle A", "Aisle B");
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.Name).Should().Equal("BIN-01", "BIN-02");
        result.StorageZones[0].StorageAisles[0].BinLocations.Select(x => x.IsActive).Should().Equal(true, false);
    }
}
