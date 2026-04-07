namespace LastMile.TMS.Application.Parcels.DTOs;

public sealed record InboundManifestDto
{
    public Guid Id { get; init; }
    public string ManifestNumber { get; init; } = string.Empty;
    public string? TruckIdentifier { get; init; }
    public Guid DepotId { get; init; }
    public string DepotName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ExpectedParcelCount { get; init; }
    public int ScannedExpectedCount { get; init; }
    public int ScannedUnexpectedCount { get; init; }
    public Guid? OpenSessionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public InboundManifestDto() { }
}

public sealed record InboundReceivingSessionDto
{
    public Guid Id { get; init; }
    public Guid ManifestId { get; init; }
    public string ManifestNumber { get; init; } = string.Empty;
    public string? TruckIdentifier { get; init; }
    public Guid DepotId { get; init; }
    public string DepotName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public string? StartedBy { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public string? ConfirmedBy { get; init; }
    public int ExpectedParcelCount { get; init; }
    public int ScannedExpectedCount { get; init; }
    public int ScannedUnexpectedCount { get; init; }
    public int RemainingExpectedCount { get; init; }
    public IReadOnlyList<InboundExpectedParcelDto> ExpectedParcels { get; init; } = [];
    public IReadOnlyList<InboundScannedParcelDto> ScannedParcels { get; init; } = [];
    public IReadOnlyList<InboundReceivingExceptionDto> Exceptions { get; init; } = [];

    public InboundReceivingSessionDto() { }
}

public sealed record InboundExpectedParcelDto
{
    public Guid ManifestLineId { get; init; }
    public Guid ParcelId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsScanned { get; init; }

    public InboundExpectedParcelDto() { }
}

public sealed record InboundScannedParcelDto
{
    public Guid Id { get; init; }
    public Guid ParcelId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string MatchType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset ScannedAt { get; init; }
    public string? ScannedBy { get; init; }

    public InboundScannedParcelDto() { }
}

public sealed record InboundReceivingExceptionDto
{
    public Guid Id { get; init; }
    public Guid? ParcelId { get; init; }
    public Guid? ManifestLineId { get; init; }
    public string ExceptionType { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public InboundReceivingExceptionDto() { }
}

public sealed record InboundParcelScanResultDto
{
    public Guid SessionId { get; init; }
    public bool IsExpected { get; init; }
    public InboundScannedParcelDto ScannedParcel { get; init; } = new();
    public InboundReceivingSessionDto Session { get; init; } = new();

    public InboundParcelScanResultDto() { }
}
