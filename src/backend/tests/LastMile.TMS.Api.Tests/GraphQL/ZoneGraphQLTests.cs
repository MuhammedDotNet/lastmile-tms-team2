using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Api.Tests.GraphQL;

[Collection(ApiTestCollection.Name)]
public class ZoneGraphQLTests : GraphQLTestBase, IAsyncLifetime
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    private static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public ZoneGraphQLTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Zones_WithAdminToken_ReturnsBoundaryGeoJson()
    {
        var zoneId = await SeedZoneAsync();
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            query ($id: UUID!) {
              zone(id: $id) {
                id
                name
                boundary
                boundaryGeoJson
                isActive
                depotId
                depotName
                createdAt
                updatedAt
              }
            }
            """,
            variables: new { id = zoneId },
            accessToken: token);

        var hasErrors = document.RootElement.TryGetProperty("errors", out var errors);
        if (hasErrors)
        {
            // Dump full error details for diagnosis
            var errorText = errors[0].GetRawText();
            Assert.Fail($"GraphQL errors: {errorText}");
        }

        var zone = document.RootElement
            .GetProperty("data")
            .GetProperty("zone");

        zone.GetProperty("id").GetString().Should().Be(zoneId.ToString());
        zone.GetProperty("boundary").GetString().Should().StartWith("POLYGON");

        using var boundaryGeoJson = JsonDocument.Parse(zone.GetProperty("boundaryGeoJson").GetString()!);
        boundaryGeoJson.RootElement.GetProperty("type").GetString().Should().Be("Polygon");
        boundaryGeoJson.RootElement.GetProperty("coordinates")[0].GetArrayLength().Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task UpdateZone_DeactivateAssignedZone_ReturnsGraphQLError()
    {
        var (zoneId, depotId) = await SeedAssignedZoneAsync();
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            mutation ($id: UUID!, $input: UpdateZoneInput!) {
              updateZone(id: $id, input: $input) {
                id
                isActive
              }
            }
            """,
            variables: new
            {
                id = zoneId,
                input = new
                {
                    name = "Assigned Zone",
                    depotId,
                    isActive = false,
                }
            },
            accessToken: token);

        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue(document.RootElement.GetRawText());
        errors[0].GetProperty("message").GetString().Should().Contain("assigned to");
    }

    [Fact]
    public async Task DeleteZone_WithAssignedBinLocation_ReturnsGraphQLError()
    {
        var (zoneId, _) = await SeedAssignedZoneAsync();
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            mutation ($id: UUID!) {
              deleteZone(id: $id)
            }
            """,
            variables: new { id = zoneId },
            accessToken: token);

        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue(document.RootElement.GetRawText());
        errors[0].GetProperty("message").GetString().Should().Contain("assigned to");
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> SeedZoneAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var depot = new Depot
        {
            Name = $"Zone Depot {Guid.NewGuid():N}",
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

        var zone = new Zone
        {
            Name = $"Zone GraphQL {Guid.NewGuid():N}",
            Boundary = CreateBoundary(),
            IsActive = true,
            Depot = depot,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        dbContext.Zones.Add(zone);
        await dbContext.SaveChangesAsync();

        return zone.Id;
    }

    private async Task<(Guid ZoneId, Guid DepotId)> SeedAssignedZoneAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var depot = new Depot
        {
            Name = $"Assigned Depot {Guid.NewGuid():N}",
            Address = new Address
            {
                Street1 = "300 Collins Street",
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

        var zone = new Zone
        {
            Name = "Assigned Zone",
            Boundary = CreateBoundary(),
            IsActive = true,
            Depot = depot,
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

        var storageAisle = new StorageAisle
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
            StorageAisle = storageAisle,
            DeliveryZone = zone,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        dbContext.AddRange(depot, zone, storageZone, storageAisle, bin);
        await dbContext.SaveChangesAsync();

        return (zone.Id, depot.Id);
    }

    private static Polygon CreateBoundary()
    {
        var boundary = GeometryFactory.CreatePolygon(
            [
                new Coordinate(144.95, -37.82),
                new Coordinate(144.98, -37.82),
                new Coordinate(144.98, -37.79),
                new Coordinate(144.95, -37.79),
                new Coordinate(144.95, -37.82),
            ]);
        boundary.SRID = 4326;
        return boundary;
    }
}
