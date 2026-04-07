using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Queries;

public sealed class GetDepotStorageLayoutQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDepotStorageLayoutQuery, DepotStorageLayoutDto?>
{
    public async Task<DepotStorageLayoutDto?> Handle(GetDepotStorageLayoutQuery request, CancellationToken cancellationToken)
    {
        var depot = await db.Depots
            .AsNoTracking()
            .Where(x => x.Id == request.DepotId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (depot is null)
        {
            return null;
        }

        var storageZones = await db.StorageZones
            .AsNoTracking()
            .Where(x => x.DepotId == request.DepotId)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.DepotId })
            .ToListAsync(cancellationToken);

        var storageZoneIds = storageZones.Select(x => x.Id).ToList();

        var storageAisles = await db.StorageAisles
            .AsNoTracking()
            .Where(x => storageZoneIds.Contains(x.StorageZoneId))
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.StorageZoneId })
            .ToListAsync(cancellationToken);

        var storageAisleIds = storageAisles.Select(x => x.Id).ToList();

        var binLocations = await db.BinLocations
            .AsNoTracking()
            .Where(x => storageAisleIds.Contains(x.StorageAisleId))
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.IsActive, x.StorageAisleId })
            .ToListAsync(cancellationToken);

        var binsByAisleId = binLocations
            .GroupBy(x => x.StorageAisleId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<BinLocationResultDto>)x
                    .Select(bin => new BinLocationResultDto
                    {
                        Id = bin.Id,
                        Name = bin.Name,
                        IsActive = bin.IsActive,
                        StorageAisleId = bin.StorageAisleId,
                    })
                    .ToList());

        var aislesByZoneId = storageAisles
            .GroupBy(x => x.StorageZoneId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<StorageAisleResultDto>)x
                    .Select(aisle => new StorageAisleResultDto
                    {
                        Id = aisle.Id,
                        Name = aisle.Name,
                        StorageZoneId = aisle.StorageZoneId,
                        BinLocations = binsByAisleId.GetValueOrDefault(aisle.Id, []),
                    })
                    .ToList());

        return new DepotStorageLayoutDto
        {
            DepotId = depot.Id,
            DepotName = depot.Name,
            StorageZones = storageZones
                .Select(zone => new StorageZoneResultDto
                {
                    Id = zone.Id,
                    Name = zone.Name,
                    DepotId = zone.DepotId,
                    StorageAisles = aislesByZoneId.GetValueOrDefault(zone.Id, []),
                })
                .ToList()
        };
    }
}
