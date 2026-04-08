using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Mappings;
using LastMile.TMS.Application.BinLocations.Support;
using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class CreateStorageAisleCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateStorageAisleCommand, StorageAisleResultDto>
{
    public async Task<StorageAisleResultDto> Handle(CreateStorageAisleCommand request, CancellationToken cancellationToken)
    {
        var name = BinLocationNameNormalizer.Normalize(request.Dto.Name);
        var normalizedName = BinLocationNameNormalizer.NormalizeForUniqueness(name);

        var storageZoneExists = await db.StorageZones
            .AnyAsync(x => x.Id == request.Dto.StorageZoneId, cancellationToken);
        if (!storageZoneExists)
        {
            throw new InvalidOperationException($"Storage zone '{request.Dto.StorageZoneId}' was not found.");
        }

        var duplicateExists = await db.StorageAisles
            .AnyAsync(
                x => x.StorageZoneId == request.Dto.StorageZoneId
                    && x.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"A storage aisle named '{name}' already exists for this storage zone.");
        }

        var entity = request.Dto.ToEntity();
        entity.Name = name;
        entity.NormalizedName = normalizedName;

        db.StorageAisles.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BinLocationPersistenceExceptionSupport.IsUniqueConstraintViolation(
                ex,
                BinLocationPersistenceExceptionSupport.StorageAisleNameIndex))
        {
            throw new InvalidOperationException(
                $"A storage aisle named '{name}' already exists for this storage zone.");
        }

        return entity.ToResultDto();
    }
}
