using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Vehicles.DTOs;

public class CreateVehicleDto
{
    public string RegistrationPlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacity { get; set; }
    public VehicleStatus Status { get; set; }
    public Guid DepotId { get; set; }
}