using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Vehicles.DTOs;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string RegistrationPlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacity { get; set; }
    public VehicleStatus Status { get; set; }
    public Guid DepotId { get; set; }
    public string DepotName { get; set; } = string.Empty;
    /// <summary>Number of completed routes (for mileage stats).</summary>
    public int RoutesCompleted { get; set; }
    /// <summary>All routes for this vehicle (any status).</summary>
    public int TotalRoutes { get; set; }
    /// <summary>Sum of distance on completed routes (km).</summary>
    public int TotalMileage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
}