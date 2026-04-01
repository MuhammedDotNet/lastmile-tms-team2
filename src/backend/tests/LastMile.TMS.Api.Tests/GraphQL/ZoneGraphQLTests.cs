using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Persistence;

namespace LastMile.TMS.Api.Tests.GraphQL;

[Collection(ApiTestCollection.Name)]
public class ZoneGraphQLTests(CustomWebApplicationFactory factory)
    : GraphQLTestBase(factory), IAsyncLifetime
{
    [Fact]
    public async Task Zones_WithAdminToken_ReturnsZones()
    {
        var token = await GetAdminAccessTokenAsync();

        using var document = await PostGraphQLAsync(
            """
            query {
              zones {
                id
                name
                boundary
                isActive
                depotId
                depotName
                createdAt
                updatedAt
              }
            }
            """,
            accessToken: token);

        var hasErrors = document.RootElement.TryGetProperty("errors", out var errors);
        if (hasErrors)
        {
            // Dump full error details for diagnosis
            var errorText = errors[0].GetRawText();
            Assert.Fail($"GraphQL errors: {errorText}");
        }

        var zones = document.RootElement
            .GetProperty("data")
            .GetProperty("zones")
            .EnumerateArray()
            .ToList();

        zones.Should().NotBeEmpty();
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
