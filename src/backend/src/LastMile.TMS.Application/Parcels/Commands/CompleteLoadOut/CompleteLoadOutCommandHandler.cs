using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class CompleteLoadOutCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IParcelUpdateNotifier parcelUpdateNotifier)
    : IRequestHandler<CompleteLoadOutCommand, CompleteLoadOutResultDto>
{
    public async Task<CompleteLoadOutResultDto> Handle(
        CompleteLoadOutCommand request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.RequireCurrentDepotIdAsync(db, currentUser, cancellationToken);

        var route = await db.Routes
            .Where(r => r.Vehicle.DepotId == depotId && r.Id == request.RouteId)
            .Include(r => r.Vehicle)
            .Include(r => r.Parcels)
            .ThenInclude(parcel => parcel.TrackingEvents)
            .Include(r => r.Parcels)
            .ThenInclude(parcel => parcel.ChangeHistory)
            .SingleOrDefaultAsync(cancellationToken);

        if (route is null)
        {
            return new CompleteLoadOutResultDto
            {
                Success = false,
                Message = "Route was not found for your depot.",
            };
        }

        if (route.Status != RouteStatus.Dispatched)
        {
            return new CompleteLoadOutResultDto
            {
                Success = false,
                Message = $"Route must be in Dispatched status to complete load-out. Current status: {route.Status}.",
                Board = await RouteLoadOutSupport.LoadBoardAsync(db, route.Id, depotId, cancellationToken)
                        ?? new RouteLoadOutBoardDto(),
            };
        }

        var loadedCount = route.Parcels.Count(p => p.Status == ParcelStatus.Loaded);
        var skippedCount = route.Parcels.Count(p => p.Status == ParcelStatus.Staged);
        var totalCount = route.Parcels.Count;

        if (skippedCount > 0 && !request.Force)
        {
            return new CompleteLoadOutResultDto
            {
                Success = false,
                Message = $"{skippedCount} of {totalCount} parcels have not been loaded. Force complete to proceed anyway.",
                LoadedCount = loadedCount,
                SkippedCount = skippedCount,
                TotalCount = totalCount,
                Board = await RouteLoadOutSupport.LoadBoardAsync(db, route.Id, depotId, cancellationToken)
                        ?? new RouteLoadOutBoardDto(),
            };
        }

        var now = DateTimeOffset.UtcNow;
        var actor = InboundReceivingSupport.GetActor(currentUser);
        var stagingLocation = RouteParcelLifecycleSupport.GetStagingAreaLocation(route.StagingArea);
        var vehicleLocation = RouteParcelLifecycleSupport.GetVehicleLocation(route.Vehicle?.RegistrationPlate);
        var updatedParcels = new List<Parcel>();

        if (request.Force)
        {
            foreach (var parcel in route.Parcels.Where(candidate => candidate.Status == ParcelStatus.Staged))
            {
                if (RouteParcelLifecycleSupport.TransitionStatus(
                    db,
                    parcel,
                    ParcelStatus.Exception,
                    now,
                    actor,
                    stagingLocation,
                    "Force completed load-out: parcel was not loaded onto vehicle."))
                {
                    updatedParcels.Add(parcel);
                }
            }
        }

        foreach (var parcel in route.Parcels.Where(candidate => candidate.Status == ParcelStatus.Loaded))
        {
            if (RouteParcelLifecycleSupport.TransitionStatus(
                db,
                parcel,
                ParcelStatus.OutForDelivery,
                now,
                actor,
                vehicleLocation,
                $"Out for delivery on route {route.Id}."))
            {
                updatedParcels.Add(parcel);
            }
        }
        route.Status = RouteStatus.InProgress;
        route.LastModifiedAt = now;
        route.LastModifiedBy = actor;

        await db.SaveChangesAsync(cancellationToken);

        foreach (var parcel in updatedParcels)
        {
            await parcelUpdateNotifier.NotifyParcelUpdatedAsync(
                new ParcelUpdateNotification(parcel.TrackingNumber, parcel.Status.ToString(), parcel.LastModifiedAt),
                cancellationToken);
        }

        var board = await RouteLoadOutSupport.LoadBoardAsync(db, route.Id, depotId, cancellationToken)
                    ?? new RouteLoadOutBoardDto();

        return new CompleteLoadOutResultDto
        {
            Success = true,
            Message = "Load-out completed. Route is now in progress.",
            LoadedCount = loadedCount,
            SkippedCount = skippedCount,
            TotalCount = totalCount,
            Board = board,
        };
    }
}
