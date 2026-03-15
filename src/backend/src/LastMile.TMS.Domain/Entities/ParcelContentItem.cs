using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class ParcelContentItem : BaseAuditableEntity
{
    public Guid ParcelId { get; set; }
    public string HsCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitValue { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Weight { get; set; }
    public WeightUnit? WeightUnit { get; set; }
    public string? CountryOfOrigin { get; set; }

    // Navigation property
    public Parcel Parcel { get; set; } = null!;
}