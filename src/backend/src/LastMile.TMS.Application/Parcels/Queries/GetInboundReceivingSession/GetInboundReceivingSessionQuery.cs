using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetInboundReceivingSessionQuery(Guid SessionId) : IRequest<InboundReceivingSessionDto?>;

public sealed class GetInboundReceivingSessionQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetInboundReceivingSessionQuery, InboundReceivingSessionDto?>
{
    public async Task<InboundReceivingSessionDto?> Handle(
        GetInboundReceivingSessionQuery request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.GetCurrentDepotIdAsync(db, currentUser, cancellationToken);
        if (depotId is null || depotId == Guid.Empty)
        {
            return null;
        }

        var session = await InboundReceivingSupport.LoadSessionAsync(db, request.SessionId, cancellationToken);
        if (session is null || session.DepotId != depotId)
        {
            return null;
        }

        return session;
    }
}
