using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.Tests.Controllers;

[Collection(ApiTestCollection.Name)]
public class DriverPhotosControllerTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
    });

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UploadPhoto_StoresInObjectStorage_AndProxyEndpointReturnsBytes()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/drivers/photo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminAccessTokenAsync());

        var payload = "driver-photo-content"u8.ToArray();
        request.Content = new MultipartFormDataContent
        {
            {
                new ByteArrayContent(payload),
                "file",
                "photo.jpg"
            }
        };

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var url = document.RootElement.GetProperty("url").GetString();
        url.Should().StartWith("/api/drivers/photo/");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var fileStorage = scope.ServiceProvider.GetRequiredService<InMemoryFileStorageService>();
            var keys = await fileStorage.ListKeysAsync("drivers/");
            keys.Should().ContainSingle();
        }

        var proxyResponse = await _client.GetAsync(url);
        proxyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        proxyResponse.Content.Headers.ContentType?.MediaType.Should().Be("image/jpeg");
        (await proxyResponse.Content.ReadAsByteArrayAsync()).Should().Equal(payload);
    }

    private async Task<string> GetAdminAccessTokenAsync()
    {
        var response = await _client.PostAsync(
            "/connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = "admin@lastmile.com",
                ["password"] = "Admin@12345",
            }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("access_token").GetString()!;
    }
}
