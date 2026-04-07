using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Mappings;
using LastMile.TMS.Application.BinLocations.Support;
using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class UpdateBinLocationCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateBinLocationCommand, BinLocationResultDto?>
{
    public async Task<BinLocationResultDto?> Handle(UpdateBinLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.BinLocations
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var name = BinLocationNameNormalizer.Normalize(request.Dto.Name);
        var normalizedName = BinLocationNameNormalizer.NormalizeForUniqueness(name);

        var storageAisleExists = await db.StorageAisles
            .AnyAsync(x => x.Id == request.Dto.StorageAisleId, cancellationToken);
        if (!storageAisleExists)
        {
            throw new InvalidOperationException($"Storage aisle '{request.Dto.StorageAisleId}' was not found.");
        }

        var duplicateExists = await db.BinLocations
            .AnyAsync(
                x => x.Id != request.Id
                    && x.StorageAisleId == request.Dto.StorageAisleId
                    && (x.NormalizedName == normalizedName || x.Name.ToUpper() == normalizedName),
                cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"A bin location named '{name}' already exists for this storage aisle.");
        }

        request.Dto.UpdateEntity(entity);
        entity.Name = name;
        entity.NormalizedName = normalizedName;

        await db.SaveChangesAsync(cancellationToken);

        return entity.ToResultDto();
    }
}
