using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Routes.DTOs;

public class RouteDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int StartMileage { get; set; }
    public int EndMileage { get; set; }
    public int TotalMileage { get; set; }
    public RouteStatus Status { get; set; }
    public int ParcelCount { get; set; }
    public int ParcelsDelivered { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateRouteDto
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public int StartMileage { get; set; }
    public List<Guid> ParcelIds { get; set; } = [];
}

public class CompleteRouteDto
{
    public int EndMileage { get; set; }
}