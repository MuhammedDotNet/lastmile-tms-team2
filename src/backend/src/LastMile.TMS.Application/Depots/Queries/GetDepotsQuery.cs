using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Depots.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Depots.Queries;

public sealed record GetDepotsQuery : IRequest<IReadOnlyList<DepotListItemDto>>;

public sealed class GetDepotsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetDepotsQuery, IReadOnlyList<DepotListItemDto>>
{
    public async Task<IReadOnlyList<DepotListItemDto>> Handle(
        GetDepotsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Depots
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DepotListItemDto { Id = d.Id, Name = d.Name })
            .ToListAsync(cancellationToken);
    }
}
