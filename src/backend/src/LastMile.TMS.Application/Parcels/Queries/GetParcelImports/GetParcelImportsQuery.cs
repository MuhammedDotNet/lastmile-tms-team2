using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetParcelImportsQuery() : IRequest<IReadOnlyList<ParcelImportHistoryDto>>;

public sealed class GetParcelImportsQueryHandler(Common.Interfaces.IAppDbContext db)
    : IRequestHandler<GetParcelImportsQuery, IReadOnlyList<ParcelImportHistoryDto>>
{
    public async Task<IReadOnlyList<ParcelImportHistoryDto>> Handle(
        GetParcelImportsQuery request,
        CancellationToken cancellationToken)
    {
        return await db.ParcelImports
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ParcelImportHistoryDto
            {
                Id = x.Id,
                FileName = x.FileName,
                FileFormat = x.FileFormat.ToString(),
                Status = x.Status.ToString(),
                TotalRows = x.TotalRows,
                ProcessedRows = x.ProcessedRows,
                ImportedRows = x.ImportedRows,
                RejectedRows = x.RejectedRows,
                CreatedAt = x.CreatedAt,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                DepotName = db.Depots
                    .Where(d => d.AddressId == x.ShipperAddressId)
                    .Select(d => d.Name)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
    }
}
