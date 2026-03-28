namespace LastMile.TMS.Api.GraphQL.Vehicles;

public sealed class CreateVehicleInput
{
    public string RegistrationPlate { get; set; } = string.Empty;
    public LastMile.TMS.Domain.Enums.VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacity { get; set; }
    public LastMile.TMS.Domain.Enums.VehicleStatus Status { get; set; }
    public Guid DepotId { get; set; }
}

public sealed class UpdateVehicleInput
{
    public string RegistrationPlate { get; set; } = string.Empty;
    public LastMile.TMS.Domain.Enums.VehicleType Type { get; set; }
    public int ParcelCapacity { get; set; }
    public decimal WeightCapacity { get; set; }
    public LastMile.TMS.Domain.Enums.VehicleStatus Status { get; set; }
    public Guid DepotId { get; set; }
}
