using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class ScanInboundParcelCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IParcelUpdateNotifier parcelUpdateNotifier)
    : IRequestHandler<ScanInboundParcelCommand, InboundParcelScanResultDto>
{
    public async Task<InboundParcelScanResultDto> Handle(
        ScanInboundParcelCommand request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.RequireCurrentDepotIdAsync(db, currentUser, cancellationToken);
        var actor = InboundReceivingSupport.GetActor(currentUser);
        var barcode = request.Barcode.Trim();

        var session = await db.InboundReceivingSessions
            .Include(x => x.Manifest)
                .ThenInclude(x => x.Depot)
            .Include(x => x.Manifest)
                .ThenInclude(x => x.Lines)
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

        if (session.Scans.Any(scan => scan.Barcode == barcode))
        {
            throw new InvalidOperationException("This parcel has already been scanned for the current receiving session.");
        }

        var parcel = await db.Parcels
            .Include(x => x.Zone)
                .ThenInclude(x => x.Depot)
            .Include(x => x.TrackingEvents)
            .SingleOrDefaultAsync(x => x.TrackingNumber == barcode, cancellationToken);

        if (parcel is null)
        {
            throw new InvalidOperationException("Scanned barcode is not registered to a parcel.");
        }

        if (parcel.Zone.DepotId != session.Manifest.DepotId)
        {
            throw new InvalidOperationException("Scanned parcel belongs to a different depot.");
        }

        if (parcel.Status != ParcelStatus.Registered)
        {
            throw new InvalidOperationException("Scanned parcel has already been received or processed.");
        }

        var manifestLine = session.Manifest.Lines.SingleOrDefault(line => line.ParcelId == parcel.Id);
        var matchType = manifestLine is null
            ? InboundReceivingMatchType.Unexpected
            : InboundReceivingMatchType.Expected;
        var now = DateTimeOffset.UtcNow;

        var scan = new InboundReceivingScan
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ParcelId = parcel.Id,
            ManifestLineId = manifestLine?.Id,
            Barcode = barcode,
            MatchType = matchType,
            CreatedAt = now,
            CreatedBy = actor,
        };

        db.InboundReceivingScans.Add(scan);

        if (matchType == InboundReceivingMatchType.Unexpected)
        {
            db.InboundReceivingExceptions.Add(new InboundReceivingException
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ParcelId = parcel.Id,
                TrackingNumber = parcel.TrackingNumber,
                Barcode = barcode,
                ExceptionType = InboundReceivingExceptionType.Unexpected,
                CreatedAt = now,
                CreatedBy = actor,
            });
        }

        parcel.TransitionTo(ParcelStatus.ReceivedAtDepot);
        parcel.LastModifiedAt = now;
        parcel.LastModifiedBy = actor;
        parcel.TrackingEvents.Add(ParcelTrackingEventFactory.CreateForParcelStatus(
            parcel.Id,
            ParcelStatus.ReceivedAtDepot,
            now,
            session.Manifest.Depot.Name,
            matchType == InboundReceivingMatchType.Expected
                ? $"Parcel received via inbound scan for manifest {session.Manifest.ManifestNumber}."
                : $"Parcel received via inbound scan outside manifest {session.Manifest.ManifestNumber}.",
            actor));

        await db.SaveChangesAsync(cancellationToken);

        await parcelUpdateNotifier.NotifyParcelUpdatedAsync(
            new ParcelUpdateNotification(parcel.TrackingNumber, parcel.Status.ToString(), parcel.LastModifiedAt),
            cancellationToken);

        var sessionDto = (await InboundReceivingSupport.LoadSessionAsync(db, session.Id, cancellationToken))!;
        var scannedParcel = sessionDto.ScannedParcels.First(scanDto => scanDto.Id == scan.Id);

        return new InboundParcelScanResultDto
        {
            SessionId = session.Id,
            IsExpected = matchType == InboundReceivingMatchType.Expected,
            ScannedParcel = scannedParcel,
            Session = sessionDto,
        };
    }
}
