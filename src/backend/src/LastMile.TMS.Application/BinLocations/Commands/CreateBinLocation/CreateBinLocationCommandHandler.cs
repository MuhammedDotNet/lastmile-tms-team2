using LastMile.TMS.Application.BinLocations.DTOs;
using LastMile.TMS.Application.BinLocations.Mappings;
using LastMile.TMS.Application.BinLocations.Support;
using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class CreateBinLocationCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateBinLocationCommand, BinLocationResultDto>
{
    public async Task<BinLocationResultDto> Handle(CreateBinLocationCommand request, CancellationToken cancellationToken)
    {
        var name = BinLocationNameNormalizer.Normalize(request.Dto.Name);
        var normalizedName = BinLocationNameNormalizer.NormalizeForUniqueness(name);

        var duplicateExists = await db.BinLocations
            .AnyAsync(
                x => x.StorageAisleId == request.Dto.StorageAisleId
                    && x.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"A bin location named '{name}' already exists for this storage aisle.");
        }

        var deliveryZoneAssignment = await BinLocationDeliveryZoneAssignmentSupport.EnsureValidAssignmentAsync(
            db,
            request.Dto.StorageAisleId,
            request.Dto.DeliveryZoneId,
            request.Dto.IsActive,
            excludingBinId: null,
            cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.Name = name;
        entity.NormalizedName = normalizedName;
        entity.DeliveryZoneId = deliveryZoneAssignment.DeliveryZoneId;

        try
        {
            db.BinLocations.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (BinLocationPersistenceExceptionSupport.IsUniqueConstraintViolation(
                ex,
                BinLocationPersistenceExceptionSupport.BinLocationNameIndex))
        {
            throw new InvalidOperationException(
                $"A bin location named '{name}' already exists for this storage aisle.");
        }
        catch (DbUpdateException ex)
            when (BinLocationPersistenceExceptionSupport.IsUniqueConstraintViolation(
                ex,
                BinLocationPersistenceExceptionSupport.BinLocationDeliveryZoneIndex))
        {
            throw new InvalidOperationException(
                "The selected delivery zone is already assigned to another active bin location.");
        }

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
