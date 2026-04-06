using System.Threading.Tasks;
using FluentAssertions;
using LastMile.TMS.Api.Tests.GraphQL;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace LastMile.TMS.Api.Tests;

[Collection(ApiTestCollection.Name)]
public class ParcelDetailHubTests(CustomWebApplicationFactory factory)
    : GraphQLTestBase(factory), IAsyncLifetime
{
    [Fact]
    public async Task ParcelUpdated_IsPublishedToSubscribers_WhenParcelStatusChanges()
    {
        var token = await GetAdminAccessTokenAsync();
        var parcel = await RegisterParcelAsync(token);

        await TransitionStatusAsync(token, parcel.Id, "RECEIVED_AT_DEPOT", "Sydney Depot", "Arrived");
        await TransitionStatusAsync(token, parcel.Id, "SORTED", "Sydney Depot", "Sorted");
        await TransitionStatusAsync(token, parcel.Id, "STAGED", "Sydney Depot", "Staged");
        await TransitionStatusAsync(token, parcel.Id, "LOADED", "Dock 2", "Loaded");
        await TransitionStatusAsync(token, parcel.Id, "OUT_FOR_DELIVERY", "Van 1", "Out for delivery");

        var updateTask = new TaskCompletionSource<ParcelUpdateMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connection = new HubConnectionBuilder()
            .WithUrl(
                new Uri(Client.BaseAddress!, "/hubs/parcels"),
                options =>
                {
                    options.Transports = HttpTransportType.LongPolling;
                    options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
            .Build();

        connection.On<ParcelUpdateMessage>("ParcelUpdated", update => updateTask.TrySetResult(update));

        await connection.StartAsync();
        await connection.InvokeAsync("SubscribeToParcel", parcel.TrackingNumber);

        await TransitionStatusAsync(token, parcel.Id, "DELIVERED", "Customer Door", "Delivered");

        var update = await updateTask.Task.WaitAsync(TimeSpan.FromSeconds(10));

        update.TrackingNumber.Should().Be(parcel.TrackingNumber);
        update.Status.Should().Be("Delivered");
        update.LastModifiedAt.Should().NotBeNull();

        await connection.DisposeAsync();
    }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<RegisteredParcel> RegisterParcelAsync(string token)
    {
        using var document = await PostGraphQLAsync(
            """
            mutation RegisterParcel($input: RegisterParcelInput!) {
              registerParcel(input: $input) {
                id
                trackingNumber
              }
            }
            """,
            variables: new
            {
                input = new
                {
                    shipperAddressId = LastMile.TMS.Persistence.DbSeeder.TestParcelShipperAddressId.ToString(),
                    recipientAddress = new
                    {
                        street1 = "45 Hub Street",
                        city = "Sydney",
                        state = "NSW",
                        postalCode = "2000",
                        countryCode = "AU",
                        isResidential = true,
                        contactName = "Hub Test",
                        phone = "+61000000001",
                        email = "hub-test@example.com"
                    },
                    serviceType = "STANDARD",
                    weight = 1.5,
                    weightUnit = "KG",
                    length = 20.0,
                    width = 10.0,
                    height = 5.0,
                    dimensionUnit = "CM",
                    declaredValue = 100.0,
                    currency = "AUD",
                    estimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(2).ToString("o")
                }
            },
            accessToken: token);

        var result = document.RootElement
            .GetProperty("data")
            .GetProperty("registerParcel");

        return new RegisteredParcel(
            result.GetProperty("id").GetString()!,
            result.GetProperty("trackingNumber").GetString()!);
    }

    private async Task TransitionStatusAsync(
        string token,
        string parcelId,
        string newStatus,
        string location,
        string description)
    {
        using var document = await PostGraphQLAsync(
            """
            mutation TransitionParcelStatus($input: TransitionParcelStatusInput!) {
              transitionParcelStatus(input: $input) {
                id
                status
              }
            }
            """,
            variables: new
            {
                input = new
                {
                    parcelId,
                    newStatus,
                    location,
                    description
                }
            },
            accessToken: token);

        document.RootElement.TryGetProperty("errors", out var errors).Should().BeFalse(errors.ToString());
    }

    private sealed record RegisteredParcel(string Id, string TrackingNumber);

    private sealed record ParcelUpdateMessage(
        string TrackingNumber,
        string Status,
        DateTimeOffset? LastModifiedAt);
}
