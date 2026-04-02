using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Parcels;

[Mapper]
public static partial class ParcelInputMapper
{
    public static partial LastMile.TMS.Application.Parcels.DTOs.RegisterParcelRecipientAddressDto ToDto(
        this RegisterParcelRecipientAddressInput input);

    public static partial LastMile.TMS.Application.Parcels.DTOs.RegisterParcelDto ToDto(this RegisterParcelInput input);

    public static partial LastMile.TMS.Application.Parcels.DTOs.UpdateParcelDto ToDto(this UpdateParcelInput input);

    private static DateTime DateTimeOffsetToUtc(DateTimeOffset value) => value.UtcDateTime;
}
