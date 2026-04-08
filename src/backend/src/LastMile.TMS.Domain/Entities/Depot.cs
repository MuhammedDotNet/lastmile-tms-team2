using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Depot: BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid AddressId { get; set; }
    public Address Address { get; set; } = null!;
    public ICollection<OperatingHours> OperatingHours { get; set; } = new List<OperatingHours>(); 

    public bool IsActive { get; set; } = true;

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();

    public ICollection<StorageZone> StorageZones { get; set; } = new List<StorageZone>();

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
