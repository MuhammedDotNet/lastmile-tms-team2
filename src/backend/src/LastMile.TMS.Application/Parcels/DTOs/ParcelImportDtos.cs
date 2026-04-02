namespace LastMile.TMS.Application.Parcels.DTOs;

public sealed record ParcelImportHistoryDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileFormat { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int ImportedRows { get; init; }
    public int RejectedRows { get; init; }
    public string? DepotName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}

public sealed record ParcelImportRowFailurePreviewDto
{
    public int RowNumber { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string OriginalRowValues { get; init; } = string.Empty;
}

public sealed record ParcelImportDetailDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileFormat { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int ImportedRows { get; init; }
    public int RejectedRows { get; init; }
    public string? DepotName { get; init; }
    public string? FailureMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public IReadOnlyList<string> CreatedTrackingNumbers { get; init; } = [];
    public IReadOnlyList<ParcelImportRowFailurePreviewDto> RowFailuresPreview { get; init; } = [];
}

public sealed record ParcelImportErrorReportRowDto
{
    public int RowNumber { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string?> Values { get; init; } =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}

public sealed record ParcelImportErrorReportDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public IReadOnlyList<ParcelImportErrorReportRowDto> Rows { get; init; } = [];
}
