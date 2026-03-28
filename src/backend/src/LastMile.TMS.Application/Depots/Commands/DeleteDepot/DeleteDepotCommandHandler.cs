using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Depots.Commands;

public sealed class DeleteDepotCommandHandler(IAppDbContext db) : IRequestHandler<DeleteDepotCommand, bool>
{
    public async Task<bool> Handle(DeleteDepotCommand request, CancellationToken cancellationToken)
    {
        var depot = await db.Depots
            .Include(d => d.Zones)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (depot is null)
            return false;

        if (depot.Zones.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete depot '{depot.Name}' because it has {depot.Zones.Count} zone(s) assigned to it. Remove or reassign the zones first.");
        }

        db.Depots.Remove(depot);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
