using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetParcelImportQuery(Guid Id) : IRequest<ParcelImportDetailDto?>;

public sealed class GetParcelImportQueryHandler(Common.Interfaces.IAppDbContext db)
    : IRequestHandler<GetParcelImportQuery, ParcelImportDetailDto?>
{
    public async Task<ParcelImportDetailDto?> Handle(GetParcelImportQuery request, CancellationToken cancellationToken)
    {
        var detail = await db.ParcelImports
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ParcelImportDetailDto
            {
                Id = x.Id,
                FileName = x.FileName,
                FileFormat = x.FileFormat.ToString(),
                Status = x.Status.ToString(),
                TotalRows = x.TotalRows,
                ProcessedRows = x.ProcessedRows,
                ImportedRows = x.ImportedRows,
                RejectedRows = x.RejectedRows,
                FailureMessage = x.FailureMessage,
                CreatedAt = x.CreatedAt,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                DepotName = db.Depots
                    .Where(d => d.AddressId == x.ShipperAddressId)
                    .Select(d => d.Name)
                    .FirstOrDefault(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (detail is null)
        {
            return null;
        }

        var createdTrackingNumbers = await db.Parcels
            .AsNoTracking()
            .Where(x => x.ParcelImportId == request.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.TrackingNumber)
            .Take(10)
            .ToListAsync(cancellationToken);

        var rowFailures = await db.ParcelImportRowFailures
            .AsNoTracking()
            .Where(x => x.ParcelImportId == request.Id)
            .OrderBy(x => x.RowNumber)
            .Select(x => new ParcelImportRowFailurePreviewDto
            {
                RowNumber = x.RowNumber,
                ErrorMessage = x.ErrorMessage,
                OriginalRowValues = x.OriginalRowValues,
            })
            .Take(10)
            .ToListAsync(cancellationToken);

        return detail with
        {
            CreatedTrackingNumbers = createdTrackingNumbers,
            RowFailuresPreview = rowFailures,
        };
    }
}
