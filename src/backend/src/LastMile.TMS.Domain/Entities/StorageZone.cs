using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class StorageZone : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public Guid DepotId { get; set; }

    public Depot Depot { get; set; } = null!;

    public ICollection<StorageAisle> StorageAisles { get; set; } = new List<StorageAisle>();
}
