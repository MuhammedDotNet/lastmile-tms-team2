using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Queries;

public sealed class GetDriverQueryHandler(
    IAppDbContext dbContext) : IRequestHandler<GetDriverQuery, Driver?>
{
    public async Task<Driver?> Handle(GetDriverQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Drivers
            .AsNoTracking()
            .Include(d => d.Zone)
            .Include(d => d.Depot)
            .Include(d => d.User)
            .Include(d => d.AvailabilitySchedule)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
    }
}
