using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Vehicle : BaseAuditableEntity
{
    public string RegistrationPlate { get; set; } = string.Empty;

    public VehicleType Type { get; set; }

    public int ParcelCapacity { get; set; }

    /// <summary>
    /// Maximum cargo weight in kilograms (kg). Used when validating route parcel assignments.
    /// </summary>
    public decimal WeightCapacity { get; set; }

    public VehicleStatus Status { get; set; }

    public Guid DepotId { get; set; }

    public Depot Depot { get; set; } = null!;
}
