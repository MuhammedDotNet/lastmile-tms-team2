using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class DeleteStorageZoneCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteStorageZoneCommand, bool>
{
    public async Task<bool> Handle(DeleteStorageZoneCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.StorageZones
            .Include(x => x.StorageAisles)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.StorageAisles.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete storage zone '{entity.Name}' because it has {entity.StorageAisles.Count} aisle(s) assigned to it. Remove or delete the aisles first.");
        }

        db.StorageZones.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
