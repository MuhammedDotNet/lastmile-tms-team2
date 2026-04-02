using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.Tests.GraphQL;

[Collection(ApiTestCollection.Name)]
public class DriverGraphQLTests : GraphQLTestBase, IAsyncLifetime
{
    public DriverGraphQLTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Drivers_WithoutToken_ReturnsAuthorizationError()
    {
        using var document = await PostGraphQLAsync(
            """
            query {
              drivers {
                id
              }
            }
            """);

        document.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("authorized");
    }

    [Fact]
    public async Task Drivers_WithAdminToken_IncludesSeededTestDriver()
    {
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            query {
              drivers {
                id
                firstName
                lastName
                displayName
                licenseNumber
                status
              }
            }
            """,
            accessToken: token);

        document.RootElement.TryGetProperty("errors", out _).Should().BeFalse(document.RootElement.GetRawText());

        var drivers = document.RootElement
            .GetProperty("data")
            .GetProperty("drivers")
            .EnumerateArray()
            .ToList();

        var testDriver = drivers.Single(d => d.GetProperty("id").GetString() == DbSeeder.TestDriverId.ToString());
        testDriver.GetProperty("firstName").GetString().Should().Be("Test");
        testDriver.GetProperty("lastName").GetString().Should().Be("Driver");
        testDriver.GetProperty("displayName").GetString().Should().Be("Test Driver");
        testDriver.GetProperty("licenseNumber").GetString().Should().Be("TEST-LIC-SEED-001");
        testDriver.GetProperty("status").GetString().Should().Be("ACTIVE");
    }

    [Fact]
    public async Task Driver_ById_ReturnsSeededDriver()
    {
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            query Driver($id: UUID!) {
              driver(id: $id) {
                id
                firstName
                lastName
                displayName
                zoneName
                depotName
              }
            }
            """,
            new { id = DbSeeder.TestDriverId },
            token);

        document.RootElement.TryGetProperty("errors", out _).Should().BeFalse(document.RootElement.GetRawText());

        var driver = document.RootElement
            .GetProperty("data")
            .GetProperty("driver");

        driver.GetProperty("id").GetString().Should().Be(DbSeeder.TestDriverId.ToString());
        driver.GetProperty("firstName").GetString().Should().Be("Test");
        driver.GetProperty("zoneName").GetString().Should().NotBeNullOrWhiteSpace();
        driver.GetProperty("depotName").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Drivers_Projection_DoesNotSelectUnrequestedScalarColumns()
    {
        var token = await GetAdminAccessTokenAsync();

        Factory.SqlCapture.Clear();

        using var document = await PostGraphQLAsync(
            """
            query {
              drivers {
                id
                firstName
              }
            }
            """,
            accessToken: token);

        document.RootElement.TryGetProperty("errors", out _).Should().BeFalse(document.RootElement.GetRawText());

        var driversSql = Factory.SqlCapture.Commands.First(c => c.Contains("FROM \"Drivers\""));
        driversSql.Should().Contain("\"FirstName\"");
        // Default sort is LastName, FirstName — LastName must appear for ORDER BY.
        driversSql.Should().Contain("\"LastName\"");
        driversSql.Should().NotContain("\"Phone\"");
        driversSql.Should().NotContain("\"Email\"");
    }

    [Fact]
    public async Task CreateDriver_WithValidInput_ReturnsDriver()
    {
        var user = await SeedUnlinkedDriverUserAsync();
        var token = await GetAdminAccessTokenAsync();
        var license = $"LIC-GQL-{Guid.NewGuid():N}";
        var expiry = DateTimeOffset.UtcNow.AddYears(2);

        using var document = await PostGraphQLAsync(
            """
            mutation CreateDriver($input: CreateDriverInput!) {
              createDriver(input: $input) {
                id
                firstName
                lastName
                licenseNumber
                status
                zoneId
                depotId
                userId
              }
            }
            """,
            new
            {
                input = new
                {
                    firstName = "GraphQL",
                    lastName = "Created",
                    licenseNumber = license,
                    licenseExpiryDate = expiry,
                    zoneId = DbSeeder.TestZoneId,
                    depotId = DbSeeder.TestDepotId,
                    status = "ACTIVE",
                    userId = user.Id,
                    availabilitySchedule = Array.Empty<object>()
                }
            },
            token);

        document.RootElement.TryGetProperty("errors", out _).Should().BeFalse(document.RootElement.GetRawText());

        var created = document.RootElement
            .GetProperty("data")
            .GetProperty("createDriver");

        var driverId = created.GetProperty("id").GetGuid();
        created.GetProperty("firstName").GetString().Should().Be("GraphQL");
        created.GetProperty("licenseNumber").GetString().Should().Be(license);
        created.GetProperty("zoneId").GetString().Should().Be(DbSeeder.TestZoneId.ToString());
        created.GetProperty("depotId").GetString().Should().Be(DbSeeder.TestDepotId.ToString());
        created.GetProperty("userId").GetString().Should().Be(user.Id.ToString());

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await db.Drivers.FindAsync(driverId);
        persisted.Should().NotBeNull();
        persisted!.FirstName.Should().Be("GraphQL");
    }

    [Fact]
    public async Task UpdateDriver_WithValidInput_UpdatesDriver()
    {
        var user = await SeedUnlinkedDriverUserAsync();
        var token = await GetAdminAccessTokenAsync();
        var license = $"LIC-GQL-{Guid.NewGuid():N}";
        var expiry = DateTimeOffset.UtcNow.AddYears(2);

        using var createDoc = await PostGraphQLAsync(
            """
            mutation CreateDriver($input: CreateDriverInput!) {
              createDriver(input: $input) {
                id
              }
            }
            """,
            new
            {
                input = new
                {
                    firstName = "Before",
                    lastName = "Update",
                    licenseNumber = license,
                    licenseExpiryDate = expiry,
                    zoneId = DbSeeder.TestZoneId,
                    depotId = DbSeeder.TestDepotId,
                    status = "ACTIVE",
                    userId = user.Id,
                    availabilitySchedule = Array.Empty<object>()
                }
            },
            token);

        createDoc.RootElement.TryGetProperty("errors", out _).Should().BeFalse(createDoc.RootElement.GetRawText());
        var driverId = createDoc.RootElement
            .GetProperty("data")
            .GetProperty("createDriver")
            .GetProperty("id")
            .GetGuid();

        using var updateDoc = await PostGraphQLAsync(
            """
            mutation UpdateDriver($id: UUID!, $input: UpdateDriverInput!) {
              updateDriver(id: $id, input: $input) {
                id
                firstName
                lastName
                licenseNumber
              }
            }
            """,
            new
            {
                id = driverId,
                input = new
                {
                    firstName = "After",
                    lastName = "Update",
                    licenseNumber = license,
                    licenseExpiryDate = expiry,
                    zoneId = DbSeeder.TestZoneId,
                    depotId = DbSeeder.TestDepotId,
                    status = "ACTIVE",
                    userId = user.Id,
                    availabilitySchedule = Array.Empty<object>()
                }
            },
            token);

        updateDoc.RootElement.TryGetProperty("errors", out _).Should().BeFalse(updateDoc.RootElement.GetRawText());
        var updated = updateDoc.RootElement
            .GetProperty("data")
            .GetProperty("updateDriver");

        updated.GetProperty("firstName").GetString().Should().Be("After");
        updated.GetProperty("lastName").GetString().Should().Be("Update");
    }

    [Fact]
    public async Task DeleteDriver_ReturnsTrueAndRemovesDriver()
    {
        var user = await SeedUnlinkedDriverUserAsync();
        var token = await GetAdminAccessTokenAsync();
        var license = $"LIC-GQL-{Guid.NewGuid():N}";
        var expiry = DateTimeOffset.UtcNow.AddYears(2);

        using var createDoc = await PostGraphQLAsync(
            """
            mutation CreateDriver($input: CreateDriverInput!) {
              createDriver(input: $input) {
                id
              }
            }
            """,
            new
            {
                input = new
                {
                    firstName = "To",
                    lastName = "Delete",
                    licenseNumber = license,
                    licenseExpiryDate = expiry,
                    zoneId = DbSeeder.TestZoneId,
                    depotId = DbSeeder.TestDepotId,
                    status = "ACTIVE",
                    userId = user.Id,
                    availabilitySchedule = Array.Empty<object>()
                }
            },
            token);

        createDoc.RootElement.TryGetProperty("errors", out _).Should().BeFalse(createDoc.RootElement.GetRawText());
        var driverId = createDoc.RootElement
            .GetProperty("data")
            .GetProperty("createDriver")
            .GetProperty("id")
            .GetGuid();

        using var deleteDoc = await PostGraphQLAsync(
            """
            mutation DeleteDriver($id: UUID!) {
              deleteDriver(id: $id)
            }
            """,
            new { id = driverId },
            token);

        deleteDoc.RootElement.TryGetProperty("errors", out _).Should().BeFalse(deleteDoc.RootElement.GetRawText());
        deleteDoc.RootElement
            .GetProperty("data")
            .GetProperty("deleteDriver")
            .GetBoolean()
            .Should()
            .BeTrue();

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Drivers.AnyAsync(d => d.Id == driverId)).Should().BeFalse();
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<ApplicationUser> SeedUnlinkedDriverUserAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"driver-gql-{Guid.NewGuid():N}@lastmile.test";
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "GraphQL",
            LastName = "User",
            PhoneNumber = "+10000000000",
            IsActive = true,
            ZoneId = DbSeeder.TestZoneId,
            DepotId = DbSeeder.TestDepotId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "tests"
        };

        var createResult = await userManager.CreateAsync(user, "DriverGql123!");
        createResult.Succeeded.Should().BeTrue(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, PredefinedRole.Driver.ToString());
        roleResult.Succeeded.Should().BeTrue(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        return user;
    }
}
