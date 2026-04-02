using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.DTOs;

public sealed record CreateParcelImportDto
{
    public Guid ShipperAddressId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public ParcelImportFileFormat FileFormat { get; init; }
    public byte[] SourceFile { get; init; } = [];
}
