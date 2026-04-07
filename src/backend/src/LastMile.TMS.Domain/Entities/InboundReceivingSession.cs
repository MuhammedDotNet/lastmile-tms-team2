using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class InboundReceivingSession : BaseAuditableEntity
{
    public Guid ManifestId { get; set; }
    public InboundReceivingSessionStatus Status { get; set; } = InboundReceivingSessionStatus.Open;
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? ConfirmedBy { get; set; }

    public InboundManifest Manifest { get; set; } = null!;
    public ICollection<InboundReceivingScan> Scans { get; set; } = new List<InboundReceivingScan>();
    public ICollection<InboundReceivingException> Exceptions { get; set; } = new List<InboundReceivingException>();
}
