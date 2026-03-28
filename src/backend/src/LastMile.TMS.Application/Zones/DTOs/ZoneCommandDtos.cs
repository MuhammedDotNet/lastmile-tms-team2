namespace LastMile.TMS.Application.Zones.DTOs;

public sealed record CreateZoneDto
{
    public string Name { get; init; } = string.Empty;
    public Guid DepotId { get; init; }
    public bool IsActive { get; init; } = true;
    public string? GeoJson { get; init; }
    public List<List<double>>? Coordinates { get; init; }
    public string? BoundaryWkt { get; init; }

    public CreateZoneDto() { }
}

public sealed record UpdateZoneDto
{
    public string Name { get; init; } = string.Empty;
    public Guid DepotId { get; init; }
    public bool IsActive { get; init; }
    public string? GeoJson { get; init; }
    public List<List<double>>? Coordinates { get; init; }
    public string? BoundaryWkt { get; init; }

    public UpdateZoneDto() { }
}
