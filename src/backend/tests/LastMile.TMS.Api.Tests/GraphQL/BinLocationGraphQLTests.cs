using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Api.Tests.GraphQL;

[Collection(ApiTestCollection.Name)]
public class BinLocationGraphQLTests : GraphQLTestBase, IAsyncLifetime
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public BinLocationGraphQLTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DepotStorageLayout_WithAdminToken_ReturnsNestedHierarchy()
    {
        var depotId = await SeedStorageHierarchyAsync();
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            query ($depotId: UUID!) {
              depotStorageLayout(depotId: $depotId) {
                depotId
                depotName
                availableDeliveryZones {
                  id
                  name
                }
                storageZones {
                  id
                  name
                  depotId
                  storageAisles {
                    id
                    name
                    storageZoneId
                    binLocations {
                      id
                      name
                      isActive
                      storageAisleId
                      deliveryZoneId
                      deliveryZoneName
                    }
                  }
                }
              }
            }
            """,
            new { depotId },
            token);

        document.RootElement.TryGetProperty("errors", out _).Should().BeFalse(document.RootElement.GetRawText());

        var layout = document.RootElement
            .GetProperty("data")
            .GetProperty("depotStorageLayout");

        layout.GetProperty("depotId").GetString().Should().Be(depotId.ToString());
        layout.GetProperty("depotName").GetString().Should().Be("GraphQL Depot");
        layout.GetProperty("availableDeliveryZones").EnumerateArray().Single()
            .GetProperty("name").GetString().Should().Be("Metro North");

        var storageZones = layout.GetProperty("storageZones").EnumerateArray().ToList();
        storageZones.Should().HaveCount(1);
        storageZones[0].GetProperty("name").GetString().Should().Be("Storage Zone A");

        var aisles = storageZones[0].GetProperty("storageAisles").EnumerateArray().ToList();
        aisles.Should().HaveCount(1);
        aisles[0].GetProperty("name").GetString().Should().Be("Aisle A");

        var bins = aisles[0].GetProperty("binLocations").EnumerateArray().ToList();
        bins.Should().HaveCount(1);
        bins[0].GetProperty("name").GetString().Should().Be("BIN-01");
        bins[0].GetProperty("isActive").GetBoolean().Should().BeTrue();
        bins[0].GetProperty("deliveryZoneName").GetString().Should().Be("Metro North");
    }

    [Fact]
    public async Task StorageHierarchyMutations_WithAdminToken_CreateUpdateAndDeleteResources()
    {
        var depotId = await SeedDepotOnlyAsync();
        var deliveryZoneId = await SeedDeliveryZoneAsync(depotId, "Metro North");
        var token = await GetAdminAccessTokenAsync();

        using var createZoneDocument = await PostGraphQLAsync(
            """
            mutation ($input: CreateStorageZoneInput!) {
              createStorageZone(input: $input) {
                id
                name
                depotId
              }
            }
            """,
            new
            {
                input = new
                {
                    depotId,
                    name = "Storage Zone A"
                }
            },
            token);

        createZoneDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(createZoneDocument.RootElement.GetRawText());
        var storageZoneId = createZoneDocument.RootElement
            .GetProperty("data")
            .GetProperty("createStorageZone")
            .GetProperty("id")
            .GetGuid();

        using var updateZoneDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!, $input: UpdateStorageZoneInput!) {
              updateStorageZone(id: $id, input: $input) {
                id
                name
              }
            }
            """,
            new
            {
                id = storageZoneId,
                input = new
                {
                    name = "Storage Zone B"
                }
            },
            token);

        updateZoneDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(updateZoneDocument.RootElement.GetRawText());
        updateZoneDocument.RootElement
            .GetProperty("data")
            .GetProperty("updateStorageZone")
            .GetProperty("name")
            .GetString()
            .Should()
            .Be("Storage Zone B");

        using var createAisleDocument = await PostGraphQLAsync(
            """
            mutation ($input: CreateStorageAisleInput!) {
              createStorageAisle(input: $input) {
                id
                name
                storageZoneId
              }
            }
            """,
            new
            {
                input = new
                {
                    storageZoneId,
                    name = "Aisle A"
                }
            },
            token);

        createAisleDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(createAisleDocument.RootElement.GetRawText());
        var storageAisleId = createAisleDocument.RootElement
            .GetProperty("data")
            .GetProperty("createStorageAisle")
            .GetProperty("id")
            .GetGuid();

        using var updateAisleDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!, $input: UpdateStorageAisleInput!) {
              updateStorageAisle(id: $id, input: $input) {
                id
                name
              }
            }
            """,
            new
            {
                id = storageAisleId,
                input = new
                {
                    name = "Aisle B"
                }
            },
            token);

        updateAisleDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(updateAisleDocument.RootElement.GetRawText());
        updateAisleDocument.RootElement
            .GetProperty("data")
            .GetProperty("updateStorageAisle")
            .GetProperty("name")
            .GetString()
            .Should()
            .Be("Aisle B");

        using var createBinDocument = await PostGraphQLAsync(
            """
            mutation ($input: CreateBinLocationInput!) {
              createBinLocation(input: $input) {
                id
                name
                isActive
                storageAisleId
                deliveryZoneId
                deliveryZoneName
              }
            }
            """,
            new
            {
                input = new
                {
                    storageAisleId,
                    name = "BIN-01",
                    isActive = true,
                    deliveryZoneId
                }
            },
            token);

        createBinDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(createBinDocument.RootElement.GetRawText());
        var createdBin = createBinDocument.RootElement
            .GetProperty("data")
            .GetProperty("createBinLocation");
        var binLocationId = createdBin.GetProperty("id").GetGuid();
        createdBin.GetProperty("deliveryZoneId").GetGuid().Should().Be(deliveryZoneId);
        createdBin.GetProperty("deliveryZoneName").GetString().Should().Be("Metro North");

        using var updateBinDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!, $input: UpdateBinLocationInput!) {
              updateBinLocation(id: $id, input: $input) {
                id
                name
                isActive
                deliveryZoneId
                deliveryZoneName
              }
            }
            """,
            new
            {
                id = binLocationId,
                input = new
                {
                    name = "BIN-02",
                    deliveryZoneId = (Guid?)null
                }
            },
            token);

        updateBinDocument.RootElement.TryGetProperty("errors", out _).Should().BeFalse(updateBinDocument.RootElement.GetRawText());
        var updatedBin = updateBinDocument.RootElement
            .GetProperty("data")
            .GetProperty("updateBinLocation");
        updatedBin.GetProperty("name").GetString().Should().Be("BIN-02");
        updatedBin.GetProperty("isActive").GetBoolean().Should().BeTrue();
        updatedBin.GetProperty("deliveryZoneId").ValueKind.Should().Be(JsonValueKind.Null);
        updatedBin.GetProperty("deliveryZoneName").ValueKind.Should().Be(JsonValueKind.Null);

        using var deleteBinDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!) {
              deleteBinLocation(id: $id)
            }
            """,
            new { id = binLocationId },
            token);

        deleteBinDocument.RootElement.GetProperty("data").GetProperty("deleteBinLocation").GetBoolean().Should().BeTrue();

        using var deleteAisleDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!) {
              deleteStorageAisle(id: $id)
            }
            """,
            new { id = storageAisleId },
            token);

        deleteAisleDocument.RootElement.GetProperty("data").GetProperty("deleteStorageAisle").GetBoolean().Should().BeTrue();

        using var deleteZoneDocument = await PostGraphQLAsync(
            """
            mutation ($id: UUID!) {
              deleteStorageZone(id: $id)
            }
            """,
            new { id = storageZoneId },
            token);

        deleteZoneDocument.RootElement.GetProperty("data").GetProperty("deleteStorageZone").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task DepotStorageLayout_WithDispatcherToken_ReturnsAuthorizationError()
    {
        var depotId = await SeedDepotOnlyAsync();
        var dispatcherToken = await CreateUserTokenAsync("dispatcher@example.com", "Dispatcher@123", "Dispatcher");

        using var document = await PostGraphQLAsync(
            """
            query ($depotId: UUID!) {
              depotStorageLayout(depotId: $depotId) {
                depotId
              }
            }
            """,
            new { depotId },
            dispatcherToken);

        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue(document.RootElement.GetRawText());
        errors[0].GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> SeedDepotOnlyAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var depot = new Depot
        {
            Name = "GraphQL Depot",
            Address = new Address
            {
                Street1 = "100 Collins Street",
                City = "Melbourne",
                State = "VIC",
                PostalCode = "3000",
                CountryCode = "AU",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "tests"
            },
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        dbContext.Depots.Add(depot);
        await dbContext.SaveChangesAsync();
        return depot.Id;
    }

    private async Task<Guid> SeedStorageHierarchyAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var depot = new Depot
        {
            Name = "GraphQL Depot",
            Address = new Address
            {
                Street1 = "200 Collins Street",
                City = "Melbourne",
                State = "VIC",
                PostalCode = "3000",
                CountryCode = "AU",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "tests"
            },
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var storageZone = new StorageZone
        {
            Name = "Storage Zone A",
            NormalizedName = Normalize("Storage Zone A"),
            Depot = depot,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var deliveryZone = new Zone
        {
            Name = "Metro North",
            Boundary = CreateBoundary(),
            IsActive = true,
            Depot = depot,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var aisle = new StorageAisle
        {
            Name = "Aisle A",
            NormalizedName = Normalize("Aisle A"),
            StorageZone = storageZone,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var bin = new BinLocation
        {
            Name = "BIN-01",
            NormalizedName = Normalize("BIN-01"),
            StorageAisle = aisle,
            IsActive = true,
            DeliveryZone = deliveryZone,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        dbContext.Depots.Add(depot);
        dbContext.Zones.Add(deliveryZone);
        dbContext.StorageZones.Add(storageZone);
        dbContext.StorageAisles.Add(aisle);
        dbContext.BinLocations.Add(bin);
        await dbContext.SaveChangesAsync();

        return depot.Id;
    }

    private async Task<Guid> SeedDeliveryZoneAsync(Guid depotId, string name)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var zone = new Zone
        {
            Name = name,
            Boundary = CreateBoundary(),
            IsActive = true,
            DepotId = depotId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        dbContext.Zones.Add(zone);
        await dbContext.SaveChangesAsync();
        return zone.Id;
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

    private async Task<string> CreateUserTokenAsync(string email, string password, string roleName)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Dispatch",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue(string.Join(", ", createResult.Errors.Select(x => x.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        roleResult.Succeeded.Should().BeTrue(string.Join(", ", roleResult.Errors.Select(x => x.Description)));

        return await GetAccessTokenAsync(email, password);
    }
}
