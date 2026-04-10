using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Application.Routes.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Commands;

public sealed class CompleteRouteCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser,
    IParcelUpdateNotifier parcelUpdateNotifier) : IRequestHandler<CompleteRouteCommand, Route?>
{
    public async Task<Route?> Handle(CompleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await dbContext.Routes
            .Include(candidate => candidate.Vehicle)
            .ThenInclude(vehicle => vehicle.Depot)
            .Include(candidate => candidate.Zone)
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.TrackingEvents)
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.ChangeHistory)
            .Include(candidate => candidate.Stops)
            .ThenInclude(stop => stop.Parcels)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

        if (route is null)
        {
            return null;
        }

        if (route.Status != RouteStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress routes can be completed.");
        }

        if (request.Dto.EndMileage < route.StartMileage)
        {
            throw new InvalidOperationException("End mileage cannot be lower than the route start mileage.");
        }

        var actor = currentUser.UserName ?? currentUser.UserId;
        var completedAt = DateTimeOffset.UtcNow;
        var deliveredParcels = route.Parcels
            .Where(parcel => parcel.Status is ParcelStatus.Sorted
                or ParcelStatus.Staged
                or ParcelStatus.Loaded
                or ParcelStatus.OutForDelivery)
            .ToList();

        route.Status = RouteStatus.Completed;
        route.EndMileage = request.Dto.EndMileage;
        route.EndDate = completedAt;
        route.LastModifiedAt = completedAt;
        route.LastModifiedBy = actor;

        foreach (var parcel in deliveredParcels)
        {
            var previousStatus = parcel.Status;
            parcel.Status = ParcelStatus.Delivered;
            parcel.ActualDeliveryDate = completedAt;
            parcel.DeliveryAttempts = Math.Max(parcel.DeliveryAttempts + 1, 1);
            parcel.LastModifiedAt = completedAt;
            parcel.LastModifiedBy = actor;

            var historyEntry = new ParcelChangeHistoryEntry
            {
                ParcelId = parcel.Id,
                Action = ParcelChangeAction.Updated,
                FieldName = "Status",
                BeforeValue = ParcelChangeSupport.FormatEnum(previousStatus),
                AfterValue = ParcelChangeSupport.FormatEnum(ParcelStatus.Delivered),
                ChangedAt = completedAt,
                ChangedBy = actor,
            };

            parcel.ChangeHistory.Add(historyEntry);
            dbContext.ParcelChangeHistoryEntries.Add(historyEntry);

            var stop = route.Stops.FirstOrDefault(candidate => candidate.Parcels.Any(stopParcel => stopParcel.Id == parcel.Id));
            var deliveryLocation = stop?.RecipientLabel;
            if (string.IsNullOrWhiteSpace(deliveryLocation))
            {
                deliveryLocation = stop is null
                    ? route.Zone.Name
                    : string.Join(
                        ", ",
                        new[] { stop.Street1, stop.City, stop.State, stop.PostalCode, stop.CountryCode }
                            .Where(value => !string.IsNullOrWhiteSpace(value))
                            .Select(value => value.Trim()));
            }

            var deliveryDescription = $"Delivered when route {route.Id} was completed.";
            parcel.TrackingEvents.Add(
                ParcelTrackingEventFactory.CreateForParcelStatus(
                    parcel.Id,
                    ParcelStatus.Delivered,
                    completedAt,
                    deliveryLocation,
                    deliveryDescription,
                    actor));
        }

        var vehicleHasOtherActiveRoutes = await dbContext.Routes
            .AsNoTracking()
            .AnyAsync(
                candidate => candidate.Id != route.Id
                    && candidate.VehicleId == route.VehicleId
                    && RouteAssignmentSupport.ActiveAssignmentStatuses.Contains(candidate.Status),
                cancellationToken);

        if (!vehicleHasOtherActiveRoutes)
        {
            route.Vehicle.DepotId = route.Zone.DepotId;
            route.Vehicle.Status = VehicleStatus.Available;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var parcel in deliveredParcels)
        {
            await parcelUpdateNotifier.NotifyParcelUpdatedAsync(
                new ParcelUpdateNotification(parcel.TrackingNumber, parcel.Status.ToString(), parcel.LastModifiedAt),
                cancellationToken);
        }

        return route;
    }
}
