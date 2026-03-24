using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Queries;

/// <summary>
/// Parcels that can be assigned to a new route (sorted or staged at depot).
/// </summary>
public record GetParcelsForRouteCreationQuery : IRequest<IReadOnlyList<ParcelOptionDto>>;

public class GetParcelsForRouteCreationQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetParcelsForRouteCreationQuery, IReadOnlyList<ParcelOptionDto>>
{
    public async Task<IReadOnlyList<ParcelOptionDto>> Handle(
        GetParcelsForRouteCreationQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = new[] { ParcelStatus.Sorted, ParcelStatus.Staged };

        return await dbContext.Parcels
            .AsNoTracking()
            .Where(p => statuses.Contains(p.Status))
            .OrderBy(p => p.TrackingNumber)
            .Select(p => new ParcelOptionDto
            {
                Id = p.Id,
                TrackingNumber = p.TrackingNumber,
                Weight = p.Weight,
                WeightUnit = p.WeightUnit
            })
            .ToListAsync(cancellationToken);
    }
}
