using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class BinLocation : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Guid? DeliveryZoneId { get; set; }

    public Guid StorageAisleId { get; set; }

    public Zone? DeliveryZone { get; set; }

    public StorageAisle StorageAisle { get; set; } = null!;
}
