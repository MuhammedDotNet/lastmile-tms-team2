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
        var finalIsActive = request.Dto.IsActive ?? entity.IsActive;
        var finalDeliveryZoneId = request.Dto.DeliveryZoneIdSpecified
            ? request.Dto.DeliveryZoneId
            : entity.DeliveryZoneId;

        var duplicateExists = await db.BinLocations
            .AnyAsync(
                x => x.Id != request.Id
                    && x.StorageAisleId == entity.StorageAisleId
                    && x.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"A bin location named '{name}' already exists for this storage aisle.");
        }

        var deliveryZoneAssignment = await BinLocationDeliveryZoneAssignmentSupport.EnsureValidAssignmentAsync(
            db,
            entity.StorageAisleId,
            finalDeliveryZoneId,
            finalIsActive,
            entity.Id,
            cancellationToken);

        entity.Name = name;
        entity.NormalizedName = normalizedName;
        entity.DeliveryZoneId = deliveryZoneAssignment.DeliveryZoneId;
        if (request.Dto.IsActive.HasValue)
        {
            entity.IsActive = request.Dto.IsActive.Value;
        }

        await db.SaveChangesAsync(cancellationToken);

        return await db.BinLocations
            .AsNoTracking()
            .Where(x => x.Id == entity.Id)
            .Select(x => new BinLocationResultDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                StorageAisleId = x.StorageAisleId,
                DeliveryZoneId = x.DeliveryZoneId,
                DeliveryZoneName = x.DeliveryZone != null ? x.DeliveryZone.Name : null,
            })
            .SingleAsync(cancellationToken);
    }
}
