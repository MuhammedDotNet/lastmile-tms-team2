using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class InboundManifest : BaseAuditableEntity
{
    public string ManifestNumber { get; set; } = string.Empty;
    public string? TruckIdentifier { get; set; }
    public Guid DepotId { get; set; }
    public InboundManifestStatus Status { get; set; } = InboundManifestStatus.Open;

    public Depot Depot { get; set; } = null!;
    public ICollection<InboundManifestLine> Lines { get; set; } = new List<InboundManifestLine>();
    public ICollection<InboundReceivingSession> Sessions { get; set; } = new List<InboundReceivingSession>();
}
