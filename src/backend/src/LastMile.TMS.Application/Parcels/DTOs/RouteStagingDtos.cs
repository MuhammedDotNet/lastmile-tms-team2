using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.DTOs;

public enum RouteStagingScanOutcome
{
    Staged,
    AlreadyStaged,
    WrongRoute,
    NotExpected,
    InvalidStatus
}

public sealed record StagingRouteDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public string VehiclePlate { get; init; } = string.Empty;
    public Guid DriverId { get; init; }
    public string DriverName { get; init; } = string.Empty;
    public RouteStatus Status { get; init; }
    public StagingArea StagingArea { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public int ExpectedParcelCount { get; init; }
    public int StagedParcelCount { get; init; }
    public int RemainingParcelCount { get; init; }

    public StagingRouteDto() { }
}

public sealed record RouteStagingExpectedParcelDto
{
    public Guid ParcelId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string Barcode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsStaged { get; init; }

    public RouteStagingExpectedParcelDto() { }
}

public sealed record RouteStagingBoardDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public string VehiclePlate { get; init; } = string.Empty;
    public Guid DriverId { get; init; }
    public string DriverName { get; init; } = string.Empty;
    public RouteStatus Status { get; init; }
    public StagingArea StagingArea { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public int ExpectedParcelCount { get; init; }
    public int StagedParcelCount { get; init; }
    public int RemainingParcelCount { get; init; }
    public IReadOnlyList<RouteStagingExpectedParcelDto> ExpectedParcels { get; init; } = [];

    public RouteStagingBoardDto() { }
}

public sealed record StageParcelForRouteResultDto
{
    public RouteStagingScanOutcome Outcome { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public Guid? ParcelId { get; init; }
    public Guid? ConflictingRouteId { get; init; }
    public StagingArea? ConflictingStagingArea { get; init; }
    public RouteStagingBoardDto Board { get; init; } = new();

    public StageParcelForRouteResultDto() { }
}
