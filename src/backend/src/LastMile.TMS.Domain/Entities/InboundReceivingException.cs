using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class InboundReceivingException : BaseAuditableEntity
{
    public Guid SessionId { get; set; }
    public Guid? ParcelId { get; set; }
    public Guid? ManifestLineId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public InboundReceivingExceptionType ExceptionType { get; set; }

    public InboundReceivingSession Session { get; set; } = null!;
    public Parcel? Parcel { get; set; }
    public InboundManifestLine? ManifestLine { get; set; }
}
