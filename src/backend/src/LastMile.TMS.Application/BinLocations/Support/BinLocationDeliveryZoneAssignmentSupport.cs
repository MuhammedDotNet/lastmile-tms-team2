using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Support;

internal static class BinLocationDeliveryZoneAssignmentSupport
{
    internal sealed record DeliveryZoneAssignmentValidationResult(
        Guid? DeliveryZoneId,
        string? DeliveryZoneName);

    public static async Task<DeliveryZoneAssignmentValidationResult> EnsureValidAssignmentAsync(
        IAppDbContext db,
        Guid storageAisleId,
        Guid? deliveryZoneId,
        bool isActive,
        Guid? excludingBinId,
        CancellationToken cancellationToken)
    {
        var storageAisle = await db.StorageAisles
            .AsNoTracking()
            .Where(x => x.Id == storageAisleId)
            .Select(x => new
            {
                x.Id,
                DepotId = x.StorageZone.DepotId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (storageAisle is null)
        {
            throw new InvalidOperationException($"Storage aisle '{storageAisleId}' was not found.");
        }

        if (!deliveryZoneId.HasValue)
        {
            return new DeliveryZoneAssignmentValidationResult(null, null);
        }

        var deliveryZone = await db.Zones
            .AsNoTracking()
            .Where(x => x.Id == deliveryZoneId.Value)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.DepotId,
                x.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (deliveryZone is null)
        {
            throw new InvalidOperationException($"Delivery zone '{deliveryZoneId}' was not found.");
        }

        if (!deliveryZone.IsActive)
        {
            throw new InvalidOperationException($"Delivery zone '{deliveryZone.Name}' is inactive and cannot be assigned to a bin.");
        }

        if (deliveryZone.DepotId != storageAisle.DepotId)
        {
            throw new InvalidOperationException(
                $"Delivery zone '{deliveryZone.Name}' must belong to the same depot as the selected storage aisle.");
        }

        if (isActive)
        {
            var assignedToAnotherActiveBin = await db.BinLocations
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id != excludingBinId
                        && x.IsActive
                        && x.DeliveryZoneId == deliveryZone.Id,
                    cancellationToken);

            if (assignedToAnotherActiveBin)
            {
                throw new InvalidOperationException(
                    $"Delivery zone '{deliveryZone.Name}' is already assigned to another active bin.");
            }
        }

        return new DeliveryZoneAssignmentValidationResult(deliveryZone.Id, deliveryZone.Name);
    }
}
