using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.DTOs;

public sealed record UpdateParcelDto
{
    public Guid Id { get; init; }
    public Guid ShipperAddressId { get; init; }
    public RegisterParcelRecipientAddressDto RecipientAddress { get; init; } = null!;
    public string? Description { get; init; }
    public ServiceType ServiceType { get; init; } = ServiceType.Standard;
    public decimal Weight { get; init; }
    public WeightUnit WeightUnit { get; init; } = WeightUnit.Kg;
    public decimal Length { get; init; }
    public decimal Width { get; init; }
    public decimal Height { get; init; }
    public DimensionUnit DimensionUnit { get; init; } = DimensionUnit.Cm;
    public decimal DeclaredValue { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime EstimatedDeliveryDate { get; init; }
    public string? ParcelType { get; init; }

    public UpdateParcelDto() { }
}
