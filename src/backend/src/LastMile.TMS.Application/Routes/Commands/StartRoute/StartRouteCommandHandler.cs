using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Commands;

public sealed class StartRouteCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser,
    IParcelUpdateNotifier parcelUpdateNotifier) : IRequestHandler<StartRouteCommand, Route?>
{
    public async Task<Route?> Handle(StartRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await dbContext.Routes
            .Include(candidate => candidate.Vehicle)
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.TrackingEvents)
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.ChangeHistory)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

        if (route is null)
        {
            return null;
        }

        if (route.Status != RouteStatus.Dispatched)
        {
            throw new InvalidOperationException("Only dispatched routes can be started.");
        }

        var now = DateTimeOffset.UtcNow;
        var actor = currentUser.UserName ?? currentUser.UserId ?? "System";
        var updatedParcels = new List<Parcel>();
        var stagingLocation = RouteParcelLifecycleSupport.GetStagingAreaLocation(route.StagingArea);
        var vehicleLocation = RouteParcelLifecycleSupport.GetVehicleLocation(route.Vehicle?.RegistrationPlate);

        foreach (var parcel in route.Parcels)
        {
            var changed = false;

            if (parcel.Status == ParcelStatus.Sorted)
            {
                changed |= RouteParcelLifecycleSupport.TransitionStatus(
                    dbContext,
                    parcel,
                    ParcelStatus.Staged,
                    now,
                    actor,
                    stagingLocation,
                    $"Staged automatically when route {route.Id} was started.");
            }

            if (parcel.Status == ParcelStatus.Staged)
            {
                changed |= RouteParcelLifecycleSupport.TransitionStatus(
                    dbContext,
                    parcel,
                    ParcelStatus.Loaded,
                    now,
                    actor,
                    stagingLocation,
                    $"Loaded onto vehicle {route.Vehicle?.RegistrationPlate ?? "assigned to the route"} when route {route.Id} was started.");
            }

            if (parcel.Status == ParcelStatus.Loaded)
            {
                changed |= RouteParcelLifecycleSupport.TransitionStatus(
                    dbContext,
                    parcel,
                    ParcelStatus.OutForDelivery,
                    now,
                    actor,
                    vehicleLocation,
                    $"Out for delivery on route {route.Id}.");
            }

            if (changed)
            {
                updatedParcels.Add(parcel);
            }
        }

        route.Status = RouteStatus.InProgress;
        route.LastModifiedAt = now;
        route.LastModifiedBy = actor;

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var parcel in updatedParcels)
        {
            await parcelUpdateNotifier.NotifyParcelUpdatedAsync(
                new ParcelUpdateNotification(parcel.TrackingNumber, parcel.Status.ToString(), parcel.LastModifiedAt),
                cancellationToken);
        }

        return route;
    }
}
