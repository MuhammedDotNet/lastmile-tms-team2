using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Zones.Commands;

public sealed class DeleteZoneCommandHandler(IAppDbContext db) : IRequestHandler<DeleteZoneCommand, bool>
{
    public async Task<bool> Handle(DeleteZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await db.Zones
            .Include(z => z.Parcels)
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

        if (zone is null)
            return false;

        if (zone.Parcels.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete zone '{zone.Name}' because it has {zone.Parcels.Count} parcel(s) assigned to it. Remove or reassign the parcels first.");
        }

        var assignedBinCount = await db.BinLocations
            .CountAsync(x => x.DeliveryZoneId == request.Id, cancellationToken);

        if (assignedBinCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete zone '{zone.Name}' because it is assigned to {assignedBinCount} bin location(s). Remove the bin assignments first.");
        }

        db.Zones.Remove(zone);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
