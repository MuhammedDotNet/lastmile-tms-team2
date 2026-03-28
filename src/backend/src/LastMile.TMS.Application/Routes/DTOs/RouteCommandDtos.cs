namespace LastMile.TMS.Application.Routes.DTOs;

public sealed record CreateRouteDto
{
    public Guid VehicleId { get; init; }
    public Guid DriverId { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public int StartMileage { get; init; }
    public List<Guid> ParcelIds { get; init; } = [];

    public CreateRouteDto() { }
}

public sealed record CompleteRouteDto
{
    public int EndMileage { get; init; }

    public CompleteRouteDto() { }
}
