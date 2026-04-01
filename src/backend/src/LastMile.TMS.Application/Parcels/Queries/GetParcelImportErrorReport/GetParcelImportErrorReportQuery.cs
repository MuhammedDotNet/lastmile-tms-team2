using System.Text.Json;
using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Queries;

public sealed record GetParcelImportErrorReportQuery(Guid Id) : IRequest<ParcelImportErrorReportDto?>;

public sealed class GetParcelImportErrorReportQueryHandler(Common.Interfaces.IAppDbContext db)
    : IRequestHandler<GetParcelImportErrorReportQuery, ParcelImportErrorReportDto?>
{
    public async Task<ParcelImportErrorReportDto?> Handle(
        GetParcelImportErrorReportQuery request,
        CancellationToken cancellationToken)
    {
        var parcelImport = await db.ParcelImports
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ParcelImportErrorReportDto
            {
                Id = x.Id,
                FileName = x.FileName,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (parcelImport is null)
        {
            return null;
        }

        var rows = await db.ParcelImportRowFailures
            .AsNoTracking()
            .Where(x => x.ParcelImportId == request.Id)
            .OrderBy(x => x.RowNumber)
            .ToListAsync(cancellationToken);

        return parcelImport with
        {
            Rows = rows.Select(x => new ParcelImportErrorReportRowDto
            {
                RowNumber = x.RowNumber,
                ErrorMessage = x.ErrorMessage,
                Values = DeserializeValues(x.OriginalRowValues),
            }).ToList(),
        };
    }

    private static IReadOnlyDictionary<string, string?> DeserializeValues(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string?>>(json) ??
               new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    }
}
