using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Mappings;
using LastMile.TMS.Application.BinLocations.Support;
using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class CreateStorageZoneCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateStorageZoneCommand, StorageZoneResultDto>
{
    public async Task<StorageZoneResultDto> Handle(CreateStorageZoneCommand request, CancellationToken cancellationToken)
    {
        var name = BinLocationNameNormalizer.Normalize(request.Dto.Name);
        var normalizedName = BinLocationNameNormalizer.NormalizeForUniqueness(name);

        var depotExists = await db.Depots
            .AnyAsync(x => x.Id == request.Dto.DepotId, cancellationToken);
        if (!depotExists)
        {
            throw new InvalidOperationException($"Depot '{request.Dto.DepotId}' was not found.");
        }

        var duplicateExists = await db.StorageZones
            .AnyAsync(
                x => x.DepotId == request.Dto.DepotId
                    && x.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"A storage zone named '{name}' already exists for this depot.");
        }

        var entity = request.Dto.ToEntity();
        entity.Name = name;
        entity.NormalizedName = normalizedName;

        db.StorageZones.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BinLocationPersistenceExceptionSupport.IsUniqueConstraintViolation(
                ex,
                BinLocationPersistenceExceptionSupport.StorageZoneNameIndex))
        {
            throw new InvalidOperationException(
                $"A storage zone named '{name}' already exists for this depot.");
        }

        return entity.ToResultDto();
    }
}
