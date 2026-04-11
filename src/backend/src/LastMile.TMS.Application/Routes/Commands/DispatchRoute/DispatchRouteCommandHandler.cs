using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Routes.Commands;

public sealed class DispatchRouteCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser,
    IParcelUpdateNotifier parcelUpdateNotifier) : IRequestHandler<DispatchRouteCommand, Route?>
{
    public async Task<Route?> Handle(DispatchRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await dbContext.Routes
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.TrackingEvents)
            .Include(candidate => candidate.Parcels)
            .ThenInclude(parcel => parcel.ChangeHistory)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

        if (route is null)
        {
            return null;
        }

        if (route.Status != RouteStatus.Draft)
        {
            throw new InvalidOperationException("Only draft routes can be dispatched.");
        }

        var now = DateTimeOffset.UtcNow;
        var actor = currentUser.UserName ?? currentUser.UserId ?? "System";
        var updatedParcels = new List<Parcel>();

        foreach (var parcel in route.Parcels.Where(candidate => candidate.Status == ParcelStatus.Sorted))
        {
            if (RouteParcelLifecycleSupport.TransitionStatus(
                dbContext,
                parcel,
                ParcelStatus.Staged,
                now,
                actor,
                RouteParcelLifecycleSupport.GetStagingAreaLocation(route.StagingArea),
                $"Prepared for route {route.Id} in staging area {route.StagingArea} when the route was dispatched."))
            {
                updatedParcels.Add(parcel);
            }
        }

        route.Status = RouteStatus.Dispatched;
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
