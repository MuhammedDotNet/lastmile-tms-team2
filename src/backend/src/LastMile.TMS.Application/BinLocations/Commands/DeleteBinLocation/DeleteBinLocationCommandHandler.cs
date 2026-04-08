using LastMile.TMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed class DeleteBinLocationCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteBinLocationCommand, bool>
{
    public async Task<bool> Handle(DeleteBinLocationCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.BinLocations
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        db.BinLocations.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
