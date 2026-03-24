using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.DTOs;

public class ParcelOptionDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public WeightUnit WeightUnit { get; set; }
}
