using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class InboundManifestLine : BaseAuditableEntity
{
    public Guid ManifestId { get; set; }
    public Guid ParcelId { get; set; }

    public InboundManifest Manifest { get; set; } = null!;
    public Parcel Parcel { get; set; } = null!;
    public ICollection<InboundReceivingScan> Scans { get; set; } = new List<InboundReceivingScan>();
    public ICollection<InboundReceivingException> Exceptions { get; set; } = new List<InboundReceivingException>();
}
