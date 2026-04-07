using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class ConfirmInboundReceivingSessionCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<ConfirmInboundReceivingSessionCommand, InboundReceivingSessionDto>
{
    public async Task<InboundReceivingSessionDto> Handle(
        ConfirmInboundReceivingSessionCommand request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.RequireCurrentDepotIdAsync(db, currentUser, cancellationToken);
        var actor = InboundReceivingSupport.GetActor(currentUser);

        var session = await db.InboundReceivingSessions
            .Include(x => x.Manifest)
                .ThenInclude(x => x.Lines)
                    .ThenInclude(x => x.Parcel)
            .Include(x => x.Exceptions)
            .Include(x => x.Scans)
            .SingleOrDefaultAsync(x => x.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException($"Inbound receiving session with ID '{request.SessionId}' was not found.");
        }

        if (session.Manifest.DepotId != depotId)
        {
            throw new InvalidOperationException("Inbound receiving session belongs to a different depot.");
        }

        if (session.Status != InboundReceivingSessionStatus.Open)
        {
            throw new InvalidOperationException("Inbound receiving session has already been confirmed.");
        }

        var expectedScannedParcelIds = session.Scans
            .Where(scan => scan.MatchType == InboundReceivingMatchType.Expected)
            .Select(scan => scan.ParcelId)
            .ToHashSet();

        var now = DateTimeOffset.UtcNow;
        foreach (var missingLine in session.Manifest.Lines.Where(line => !expectedScannedParcelIds.Contains(line.ParcelId)))
        {
            var exists = session.Exceptions.Any(exceptionItem =>
                exceptionItem.ExceptionType == InboundReceivingExceptionType.Missing
                && exceptionItem.Barcode == missingLine.Parcel.TrackingNumber);

            if (exists)
            {
                continue;
            }

            db.InboundReceivingExceptions.Add(new InboundReceivingException
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ParcelId = missingLine.ParcelId,
                ManifestLineId = missingLine.Id,
                TrackingNumber = missingLine.Parcel.TrackingNumber,
                Barcode = missingLine.Parcel.TrackingNumber,
                ExceptionType = InboundReceivingExceptionType.Missing,
                CreatedAt = now,
                CreatedBy = actor,
            });
        }

        session.Status = InboundReceivingSessionStatus.Confirmed;
        session.ConfirmedAt = now;
        session.ConfirmedBy = actor;
        session.LastModifiedAt = now;
        session.LastModifiedBy = actor;
        session.Manifest.Status = InboundManifestStatus.Closed;
        session.Manifest.LastModifiedAt = now;
        session.Manifest.LastModifiedBy = actor;

        await db.SaveChangesAsync(cancellationToken);

        return (await InboundReceivingSupport.LoadSessionAsync(db, session.Id, cancellationToken))!;
    }
}
