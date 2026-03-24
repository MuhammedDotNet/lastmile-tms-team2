using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.Tests.Controllers;

public class VehiclesControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<string> GetAccessTokenAsync()
    {
        var body = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "admin@lastmile.com"),
            new KeyValuePair<string, string>("password", "Admin@12345")
        });

        var response = await _client.PostAsync("/connect/token", body);
        var json = await ParseJsonAsync(response);
        return json["access_token"].GetString()!;
    }

    private static async Task<Dictionary<string, JsonElement>> ParseJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content)!;
    }

    // ── POST /api/vehicles Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateVehicle_WithoutToken_Returns401()
    {
        // Arrange
        var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/vehicles", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_Returns201()
    {
        // Arrange: use unique plate to avoid conflicts when tests run in parallel
        var token = await GetAccessTokenAsync();
        var plate = $"TEST-{Guid.NewGuid():N}"[..20];
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plate,
                    type = 0,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = 0,
                    depotId = "00000000-0000-0000-0000-000000000001"
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, content);

        var json = await ParseJsonAsync(response);
        json.Should().ContainKey("id");
        json["registrationPlate"].GetString().Should().Be(plate);
        json.Should().ContainKey("createdAt");
        json["createdAt"].GetDateTimeOffset().Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateVehicle_WithDuplicatePlate_Returns400()
    {
        // Arrange: create first vehicle, then try duplicate plate
        var token = await GetAccessTokenAsync();
        var plate = $"DUP-{Guid.NewGuid():N}"[..20];
        var createBody = JsonSerializer.Serialize(new
        {
            registrationPlate = plate,
            type = (int)VehicleType.Van,
            parcelCapacity = 10,
            weightCapacity = 100.0m,
            status = (int)VehicleStatus.Available,
            depotId = "00000000-0000-0000-0000-000000000001"
        });

        var first = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(createBody, System.Text.Encoding.UTF8, "application/json")
        };
        var firstResponse = await _client.SendAsync(first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(createBody, System.Text.Encoding.UTF8, "application/json")
        };

        // Act
        var response = await _client.SendAsync(duplicate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateVehicle_WithInvalidData_Returns400()
    {
        // Arrange: empty plate, zero capacity → FluentValidation fails
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = "",
                    type = 0,
                    parcelCapacity = 0,
                    weightCapacity = -1m,
                    status = 0,
                    depotId = "00000000-0000-0000-0000-000000000001"
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("errors");
    }

    // ── GET /api/vehicles Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task GetVehicles_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/vehicles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehicles_WithValidToken_Returns200()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);

        var json = await ParseJsonAsync(response);
        json.Should().ContainKey("items");
        json.Should().ContainKey("totalCount");
        json.Should().ContainKey("page");
    }

    [Fact]
    public async Task GetVehicles_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/vehicles?status=0")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/vehicles/{id} Tests ─────────────────────────────────────────────

    [Fact]
    public async Task GetVehicleById_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/vehicles/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehicleById_WithInvalidId_Returns404()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/vehicles/99999999-9999-9999-9999-999999999999")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/vehicles/{id} Tests ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateVehicle_WithoutToken_Returns401()
    {
        // Arrange
        var body = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/vehicles/00000000-0000-0000-0000-000000000001", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateVehicle_WithDuplicatePlate_Returns400()
    {
        // Arrange: create two vehicles, then try to update second to first's plate
        var token = await GetAccessTokenAsync();
        var depotId = "00000000-0000-0000-0000-000000000001";
        var plateA = $"UPD-DUP-A-{Guid.NewGuid():N}"[..25];
        var plateB = $"UPD-DUP-B-{Guid.NewGuid():N}"[..25];

        var createA = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plateA,
                    type = (int)VehicleType.Van,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = (int)VehicleStatus.Available,
                    depotId
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var respA = await _client.SendAsync(createA);
        respA.StatusCode.Should().Be(HttpStatusCode.Created);
        var idA = GetIdFromLocation(respA.Headers.Location);

        var createB = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plateB,
                    type = (int)VehicleType.Van,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = (int)VehicleStatus.Available,
                    depotId
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var respB = await _client.SendAsync(createB);
        respB.StatusCode.Should().Be(HttpStatusCode.Created);
        var idB = GetIdFromLocation(respB.Headers.Location);

        var updateBody = new StringContent(
            JsonSerializer.Serialize(new
            {
                registrationPlate = plateA, // duplicate of vehicle A
                type = (int)VehicleType.Van,
                parcelCapacity = 10,
                weightCapacity = 100.0m,
                status = (int)VehicleStatus.Available,
                depotId
            }),
            System.Text.Encoding.UTF8,
            "application/json");
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/vehicles/{idB}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = updateBody
        };

        // Act
        var response = await _client.SendAsync(updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateVehicle_ToAvailable_WithActivePlannedRoute_Returns400()
    {
        var token = await GetAccessTokenAsync();
        var depotId = DbSeeder.TestDepotId;
        var plate = $"UPD-AV-{Guid.NewGuid():N}"[..25];

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plate,
                    type = (int)VehicleType.Van,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = (int)VehicleStatus.Available,
                    depotId = depotId.ToString()
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var createResp = await _client.SendAsync(createRequest);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var vehicleId = GetIdFromLocation(createResp.Headers.Location);

        await using (var scope = factory.Server.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var route = new Route
            {
                VehicleId = vehicleId,
                DriverId = DbSeeder.TestDriverId,
                StartDate = DateTimeOffset.UtcNow,
                StartMileage = 0,
                Status = RouteStatus.Planned
            };
            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        var updateBody = new StringContent(
            JsonSerializer.Serialize(new
            {
                registrationPlate = plate,
                type = (int)VehicleType.Van,
                parcelCapacity = 10,
                weightCapacity = 100.0m,
                status = (int)VehicleStatus.Available,
                depotId = depotId.ToString()
            }),
            System.Text.Encoding.UTF8,
            "application/json");
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/vehicles/{vehicleId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = updateBody
        };

        var response = await _client.SendAsync(updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Available");
        content.Should().Contain("route");
    }

    [Fact]
    public async Task UpdateVehicle_ToAvailable_WithOnlyCompletedRoute_Returns200()
    {
        var token = await GetAccessTokenAsync();
        var depotId = DbSeeder.TestDepotId;
        var plate = $"UPD-OK-{Guid.NewGuid():N}"[..25];

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plate,
                    type = (int)VehicleType.Van,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = (int)VehicleStatus.Available,
                    depotId = depotId.ToString()
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var createResp = await _client.SendAsync(createRequest);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var vehicleId = GetIdFromLocation(createResp.Headers.Location);

        await using (var scope = factory.Server.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var route = new Route
            {
                VehicleId = vehicleId,
                DriverId = DbSeeder.TestDriverId,
                StartDate = DateTimeOffset.UtcNow,
                StartMileage = 0,
                EndMileage = 100,
                Status = RouteStatus.Completed
            };
            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        var updateBody = new StringContent(
            JsonSerializer.Serialize(new
            {
                registrationPlate = plate,
                type = (int)VehicleType.Van,
                parcelCapacity = 10,
                weightCapacity = 100.0m,
                status = (int)VehicleStatus.Available,
                depotId = depotId.ToString()
            }),
            System.Text.Encoding.UTF8,
            "application/json");
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/vehicles/{vehicleId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = updateBody
        };

        var response = await _client.SendAsync(updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static Guid GetIdFromLocation(Uri? location)
    {
        if (location is null)
            throw new InvalidOperationException("Expected Location header");
        var segments = location.AbsolutePath.TrimEnd('/').Split('/');
        return Guid.Parse(segments[^1]);
    }

    // ── DELETE /api/vehicles/{id} Tests ───────────────────────────────────────────

    [Fact]
    public async Task DeleteVehicle_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.DeleteAsync("/api/vehicles/00000000-0000-0000-0000-000000000001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteVehicle_WithInvalidId_Returns404()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/vehicles/99999999-9999-9999-9999-999999999999")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteVehicle_WithActiveRoutes_Returns400()
    {
        // Arrange: create vehicle, add route with Planned status via DbContext, then try delete
        var token = await GetAccessTokenAsync();
        var depotId = DbSeeder.TestDepotId;
        var plate = $"DEL-ACT-{Guid.NewGuid():N}"[..25];

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/vehicles")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    registrationPlate = plate,
                    type = (int)VehicleType.Van,
                    parcelCapacity = 10,
                    weightCapacity = 100.0m,
                    status = (int)VehicleStatus.Available,
                    depotId = depotId.ToString()
                }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        var createResp = await _client.SendAsync(createRequest);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var vehicleId = GetIdFromLocation(createResp.Headers.Location);

        await using (var scope = factory.Server.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var route = new Route
            {
                VehicleId = vehicleId,
                DriverId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                StartMileage = 0,
                Status = RouteStatus.Planned
            };
            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/vehicles/{vehicleId}")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        // Act
        var response = await _client.SendAsync(deleteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("active routes");
    }
}