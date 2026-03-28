using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Vehicles.DTOs;

public sealed record CreateVehicleDto
{
    public string RegistrationPlate { get; init; } = string.Empty;
    public VehicleType Type { get; init; }
    public int ParcelCapacity { get; init; }
    public decimal WeightCapacity { get; init; }
    public VehicleStatus Status { get; init; }
    public Guid DepotId { get; init; }

    public CreateVehicleDto() { }
}

public sealed record UpdateVehicleDto
{
    public string RegistrationPlate { get; init; } = string.Empty;
    public VehicleType Type { get; init; }
    public int ParcelCapacity { get; init; }
    public decimal WeightCapacity { get; init; }
    public VehicleStatus Status { get; init; }
    public Guid DepotId { get; init; }

    public UpdateVehicleDto() { }
}
