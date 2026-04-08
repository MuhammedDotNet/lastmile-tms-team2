using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetStagingRoutesQuery : IRequest<IReadOnlyList<StagingRouteDto>>;

public sealed class GetStagingRoutesQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetStagingRoutesQuery, IReadOnlyList<StagingRouteDto>>
{
    public async Task<IReadOnlyList<StagingRouteDto>> Handle(
        GetStagingRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.GetCurrentDepotIdAsync(db, currentUser, cancellationToken);
        if (depotId is null || depotId == Guid.Empty)
        {
            return [];
        }

        return await RouteStagingSupport.GetActiveDepotRoutes(db, depotId.Value)
            .OrderBy(route => route.StartDate)
            .Select(route => new StagingRouteDto
            {
                Id = route.Id,
                VehicleId = route.VehicleId,
                VehiclePlate = route.Vehicle.RegistrationPlate,
                DriverId = route.DriverId,
                DriverName = $"{route.Driver.FirstName} {route.Driver.LastName}".Trim(),
                Status = route.Status,
                StagingArea = route.StagingArea,
                StartDate = route.StartDate,
                ExpectedParcelCount = route.Parcels.Count,
                StagedParcelCount = route.Parcels.Count(parcel => parcel.Status == ParcelStatus.Staged),
                RemainingParcelCount = route.Parcels.Count(parcel => parcel.Status != ParcelStatus.Staged),
            })
            .ToArrayAsync(cancellationToken);
    }
}
