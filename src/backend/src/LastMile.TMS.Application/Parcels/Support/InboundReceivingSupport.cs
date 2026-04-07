using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Support;

internal static class InboundReceivingSupport
{
    public static async Task<Guid?> GetCurrentDepotIdAsync(
        IAppDbContext db,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
        {
            return null;
        }

        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.DepotId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static async Task<Guid> RequireCurrentDepotIdAsync(
        IAppDbContext db,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var depotId = await GetCurrentDepotIdAsync(db, currentUser, cancellationToken);
        if (depotId is null || depotId == Guid.Empty)
        {
            throw new InvalidOperationException("Inbound receiving requires an assigned depot.");
        }

        return depotId.Value;
    }

    public static string GetActor(ICurrentUserService currentUser) =>
        currentUser.UserName ?? currentUser.UserId ?? "System";

    public static async Task<InboundReceivingSessionDto?> LoadSessionAsync(
        IAppDbContext db,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var session = await db.InboundReceivingSessions
            .AsNoTracking()
            .Where(x => x.Id == sessionId)
            .Include(x => x.Manifest)
                .ThenInclude(x => x.Depot)
            .Include(x => x.Manifest)
                .ThenInclude(x => x.Lines)
                    .ThenInclude(x => x.Parcel)
            .Include(x => x.Scans)
                .ThenInclude(x => x.Parcel)
            .Include(x => x.Exceptions)
            .SingleOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        return MapSession(session);
    }

    public static InboundReceivingSessionDto MapSession(InboundReceivingSession session)
    {
        var expectedScannedParcelIds = session.Scans
            .Where(scan => scan.MatchType == InboundReceivingMatchType.Expected)
            .Select(scan => scan.ParcelId)
            .ToHashSet();

        var expectedParcels = session.Manifest.Lines
            .OrderBy(line => line.Parcel.TrackingNumber)
            .Select(line => new InboundExpectedParcelDto
            {
                ManifestLineId = line.Id,
                ParcelId = line.ParcelId,
                TrackingNumber = line.Parcel.TrackingNumber,
                Barcode = line.Parcel.TrackingNumber,
                Status = line.Parcel.Status.ToString(),
                IsScanned = expectedScannedParcelIds.Contains(line.ParcelId),
            })
            .ToArray();

        var scannedParcels = session.Scans
            .OrderByDescending(scan => scan.CreatedAt)
            .Select(scan => new InboundScannedParcelDto
            {
                Id = scan.Id,
                ParcelId = scan.ParcelId,
                TrackingNumber = scan.Parcel.TrackingNumber,
                Barcode = scan.Barcode,
                MatchType = scan.MatchType.ToString(),
                Status = scan.Parcel.Status.ToString(),
                ScannedAt = scan.CreatedAt,
                ScannedBy = scan.CreatedBy,
            })
            .ToArray();

        var exceptions = session.Exceptions
            .OrderBy(exceptionItem => exceptionItem.CreatedAt)
            .Select(exceptionItem => new InboundReceivingExceptionDto
            {
                Id = exceptionItem.Id,
                ParcelId = exceptionItem.ParcelId,
                ManifestLineId = exceptionItem.ManifestLineId,
                ExceptionType = exceptionItem.ExceptionType.ToString(),
                TrackingNumber = exceptionItem.TrackingNumber,
                Barcode = exceptionItem.Barcode,
                CreatedAt = exceptionItem.CreatedAt,
            })
            .ToArray();

        var scannedExpectedCount = session.Scans.Count(scan => scan.MatchType == InboundReceivingMatchType.Expected);
        var scannedUnexpectedCount = session.Scans.Count(scan => scan.MatchType == InboundReceivingMatchType.Unexpected);

        return new InboundReceivingSessionDto
        {
            Id = session.Id,
            ManifestId = session.ManifestId,
            ManifestNumber = session.Manifest.ManifestNumber,
            TruckIdentifier = session.Manifest.TruckIdentifier,
            DepotId = session.Manifest.DepotId,
            DepotName = session.Manifest.Depot.Name,
            Status = session.Status.ToString(),
            StartedAt = session.CreatedAt,
            StartedBy = session.CreatedBy,
            ConfirmedAt = session.ConfirmedAt,
            ConfirmedBy = session.ConfirmedBy,
            ExpectedParcelCount = session.Manifest.Lines.Count,
            ScannedExpectedCount = scannedExpectedCount,
            ScannedUnexpectedCount = scannedUnexpectedCount,
            RemainingExpectedCount = session.Status == InboundReceivingSessionStatus.Confirmed
                ? 0
                : Math.Max(0, session.Manifest.Lines.Count - scannedExpectedCount),
            ExpectedParcels = expectedParcels,
            ScannedParcels = scannedParcels,
            Exceptions = exceptions,
        };
    }
}
