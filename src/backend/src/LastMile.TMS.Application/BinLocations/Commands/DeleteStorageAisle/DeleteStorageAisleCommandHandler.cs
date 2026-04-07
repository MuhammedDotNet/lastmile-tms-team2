using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class DeleteStorageAisleCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteStorageAisleCommand, bool>
{
    public async Task<bool> Handle(DeleteStorageAisleCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.StorageAisles
            .Include(x => x.BinLocations)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.BinLocations.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete storage aisle '{entity.Name}' because it has {entity.BinLocations.Count} bin(s) assigned to it. Remove or delete the bins first.");
        }

        db.StorageAisles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
