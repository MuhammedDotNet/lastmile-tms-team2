using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Drivers.Queries;

public record GetDriversQuery(Guid? DepotId = null) : IRequest<IReadOnlyList<DriverListItemDto>>;

public class GetDriversQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetDriversQuery, IReadOnlyList<DriverListItemDto>>
{
    public async Task<IReadOnlyList<DriverListItemDto>> Handle(
        GetDriversQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Drivers
            .AsNoTracking()
            .Where(d => d.Status == DriverStatus.Active);

        if (request.DepotId.HasValue)
        {
            query = query.Where(d => d.DepotId == request.DepotId.Value);
        }

        return await query
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Select(d => new DriverListItemDto
            {
                Id = d.Id,
                DisplayName = d.FirstName + " " + d.LastName
            })
            .ToListAsync(cancellationToken);
    }
}
