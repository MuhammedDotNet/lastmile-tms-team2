using LastMile.TMS.Application.Parcels.Services;
using Microsoft.AspNetCore.SignalR;

namespace LastMile.TMS.Api.Hubs;

public sealed class SignalRParcelUpdateNotifier(IHubContext<ParcelUpdatesHub> hubContext)
    : IParcelUpdateNotifier
{
    public Task NotifyParcelUpdatedAsync(
        ParcelUpdateNotification notification,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(ParcelUpdatesHub.GetGroupName(notification.TrackingNumber))
            .SendAsync("ParcelUpdated", notification, cancellationToken);
}
