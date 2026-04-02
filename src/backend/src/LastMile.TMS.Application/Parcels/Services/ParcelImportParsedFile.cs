namespace LastMile.TMS.Application.Parcels.Services;

public sealed record ParcelImportParsedFile(
    int TotalRows,
    IReadOnlyList<ParcelImportParsedRow> Rows);

public sealed record ParcelImportParsedRow(
    int RowNumber,
    IReadOnlyDictionary<string, string?> Values);
