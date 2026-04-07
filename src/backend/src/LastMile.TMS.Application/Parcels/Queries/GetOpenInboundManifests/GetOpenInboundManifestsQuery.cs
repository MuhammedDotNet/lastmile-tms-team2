using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetOpenInboundManifestsQuery : IRequest<IReadOnlyList<InboundManifestDto>>;

public sealed class GetOpenInboundManifestsQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetOpenInboundManifestsQuery, IReadOnlyList<InboundManifestDto>>
{
    public async Task<IReadOnlyList<InboundManifestDto>> Handle(
        GetOpenInboundManifestsQuery request,
        CancellationToken cancellationToken)
    {
        var depotId = await InboundReceivingSupport.GetCurrentDepotIdAsync(db, currentUser, cancellationToken);
        if (depotId is null || depotId == Guid.Empty)
        {
            return [];
        }

        var manifests = await db.InboundManifests
            .AsNoTracking()
            .Where(x => x.DepotId == depotId && x.Status == InboundManifestStatus.Open)
            .Include(x => x.Depot)
            .Include(x => x.Lines)
            .Include(x => x.Sessions)
                .ThenInclude(x => x.Scans)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return manifests
            .Select(manifest =>
            {
                var openSession = manifest.Sessions
                    .OrderByDescending(session => session.CreatedAt)
                    .FirstOrDefault(session => session.Status == InboundReceivingSessionStatus.Open);

                return new InboundManifestDto
                {
                    Id = manifest.Id,
                    ManifestNumber = manifest.ManifestNumber,
                    TruckIdentifier = manifest.TruckIdentifier,
                    DepotId = manifest.DepotId,
                    DepotName = manifest.Depot.Name,
                    Status = manifest.Status.ToString(),
                    ExpectedParcelCount = manifest.Lines.Count,
                    ScannedExpectedCount = openSession?.Scans.Count(scan => scan.MatchType == InboundReceivingMatchType.Expected) ?? 0,
                    ScannedUnexpectedCount = openSession?.Scans.Count(scan => scan.MatchType == InboundReceivingMatchType.Unexpected) ?? 0,
                    OpenSessionId = openSession?.Id,
                    CreatedAt = manifest.CreatedAt,
                };
            })
            .ToArray();
    }
}
