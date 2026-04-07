using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class InboundReceivingScan : BaseAuditableEntity
{
    public Guid SessionId { get; set; }
    public Guid ParcelId { get; set; }
    public Guid? ManifestLineId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public InboundReceivingMatchType MatchType { get; set; }

    public InboundReceivingSession Session { get; set; } = null!;
    public Parcel Parcel { get; set; } = null!;
    public InboundManifestLine? ManifestLine { get; set; }
}
