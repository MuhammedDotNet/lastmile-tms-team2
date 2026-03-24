using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Route : BaseAuditableEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public int StartMileage { get; set; }
    public int EndMileage { get; set; }

    public RouteStatus Status { get; set; }

    public ICollection<Parcel> Parcels { get; set; } = new List<Parcel>();

    /// <summary>
    /// Calculated total mileage for this route
    /// </summary>
    public int TotalMileage => EndMileage > 0 ? EndMileage - StartMileage : 0;

    /// <summary>
    /// Number of parcels delivered on this route
    /// </summary>
    public int ParcelsDelivered => Parcels?.Count(p => p.Status == ParcelStatus.Delivered) ?? 0;

    /// <summary>
    /// Number of parcels assigned to this route
    /// </summary>
    public int ParcelCount => Parcels?.Count ?? 0;
}