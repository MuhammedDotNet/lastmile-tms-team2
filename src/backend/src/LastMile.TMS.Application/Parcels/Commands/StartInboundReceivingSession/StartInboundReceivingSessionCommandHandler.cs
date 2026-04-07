using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class StartInboundReceivingSessionCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<StartInboundReceivingSessionCommand, InboundReceivingSessionDto>
{
    public async Task<InboundReceivingSessionDto> Handle(
        StartInboundReceivingSessionCommand request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.RequireCurrentDepotIdAsync(db, currentUser, cancellationToken);

        var manifest = await db.InboundManifests
            .AsNoTracking()
            .Where(x => x.Id == request.ManifestId)
            .Select(x => new
            {
                x.Id,
                x.DepotId,
                x.Status,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (manifest is null)
        {
            throw new InvalidOperationException($"Inbound manifest with ID '{request.ManifestId}' was not found.");
        }

        if (manifest.DepotId != depotId)
        {
            throw new InvalidOperationException("Inbound manifest belongs to a different depot.");
        }

        if (manifest.Status != InboundManifestStatus.Open)
        {
            throw new InvalidOperationException("Inbound manifest is not open for receiving.");
        }

        var existingSessionId = await db.InboundReceivingSessions
            .AsNoTracking()
            .Where(x => x.ManifestId == request.ManifestId && x.Status == InboundReceivingSessionStatus.Open)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingSessionId is not null)
        {
            return (await InboundReceivingSupport.LoadSessionAsync(db, existingSessionId.Value, cancellationToken))!;
        }

        var session = new InboundReceivingSession
        {
            Id = Guid.NewGuid(),
            ManifestId = request.ManifestId,
            Status = InboundReceivingSessionStatus.Open,
            CreatedBy = InboundReceivingSupport.GetActor(currentUser),
        };

        db.InboundReceivingSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return (await InboundReceivingSupport.LoadSessionAsync(db, session.Id, cancellationToken))!;
    }
}
