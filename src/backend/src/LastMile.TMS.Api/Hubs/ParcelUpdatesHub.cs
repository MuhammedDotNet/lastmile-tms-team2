using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LastMile.TMS.Api.Hubs;

[Authorize(Roles = "OperationsManager,Admin,Dispatcher,WarehouseOperator")]
public sealed class ParcelUpdatesHub : Hub
{
    public static string GetGroupName(string trackingNumber) =>
        $"parcel:{trackingNumber.Trim().ToUpperInvariant()}";

    public Task SubscribeToParcel(string trackingNumber) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(trackingNumber));

    public Task UnsubscribeFromParcel(string trackingNumber) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(trackingNumber));
}
