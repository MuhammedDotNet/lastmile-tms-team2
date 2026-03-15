using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class TrackingEvent : BaseAuditableEntity
{
    public Guid ParcelId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public EventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Operator { get; set; }
    public ExceptionReason? DelayReason { get; set; }

    // Navigation property
    public Parcel Parcel { get; set; } = null!;
}