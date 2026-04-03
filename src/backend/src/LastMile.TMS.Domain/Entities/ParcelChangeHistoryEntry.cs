using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class ParcelChangeHistoryEntry : BaseEntity
{
    public Guid ParcelId { get; set; }
    public ParcelChangeAction Action { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }

    public Parcel Parcel { get; set; } = null!;
}
