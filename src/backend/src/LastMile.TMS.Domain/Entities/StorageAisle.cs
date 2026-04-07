using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class StorageAisle : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public Guid StorageZoneId { get; set; }

    public StorageZone StorageZone { get; set; } = null!;

    public ICollection<BinLocation> BinLocations { get; set; } = new List<BinLocation>();
}
