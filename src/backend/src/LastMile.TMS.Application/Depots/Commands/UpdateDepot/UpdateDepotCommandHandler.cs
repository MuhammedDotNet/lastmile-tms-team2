using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Depots.Mappings;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Depots.Commands;

public sealed class UpdateDepotCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateDepotCommand, Depot?>
{
    public async Task<Depot?> Handle(UpdateDepotCommand request, CancellationToken cancellationToken)
    {
        var depot = await db.Depots
            .Include(d => d.Address)
            .Include(d => d.OperatingHours)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (depot is null)
            return null;

        request.Dto.UpdateEntity(depot);

        if (request.Dto.Address is not null)
        {
            request.Dto.Address.UpdateEntity(depot.Address);
            depot.Address.CountryCode = depot.Address.CountryCode.ToUpperInvariant();
        }

        if (request.Dto.OperatingHours is not null)
        {
            depot.OperatingHours.Clear();
            foreach (var hours in request.Dto.OperatingHours)
            {
                var operatingHours = hours.ToEntity();
                operatingHours.DepotId = depot.Id;
                depot.OperatingHours.Add(operatingHours);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return depot;
    }
}
