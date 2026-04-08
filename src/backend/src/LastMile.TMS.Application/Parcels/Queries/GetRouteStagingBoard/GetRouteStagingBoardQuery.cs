using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetRouteStagingBoardQuery(Guid RouteId) : IRequest<RouteStagingBoardDto?>;

public sealed class GetRouteStagingBoardQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetRouteStagingBoardQuery, RouteStagingBoardDto?>
{
    public async Task<RouteStagingBoardDto?> Handle(
        GetRouteStagingBoardQuery request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.GetCurrentDepotIdAsync(db, currentUser, cancellationToken);
        if (depotId is null || depotId == Guid.Empty)
        {
            return null;
        }

        return await RouteStagingSupport.LoadBoardAsync(db, request.RouteId, depotId.Value, cancellationToken);
    }
}
