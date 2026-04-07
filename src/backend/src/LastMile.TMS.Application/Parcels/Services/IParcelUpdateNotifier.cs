namespace LastMile.TMS.Application.Parcels.Services;

public interface IParcelUpdateNotifier
{
    Task NotifyParcelUpdatedAsync(ParcelUpdateNotification notification, CancellationToken cancellationToken = default);
}

public sealed record ParcelUpdateNotification(
    string TrackingNumber,
    string Status,
    DateTimeOffset? LastModifiedAt);
