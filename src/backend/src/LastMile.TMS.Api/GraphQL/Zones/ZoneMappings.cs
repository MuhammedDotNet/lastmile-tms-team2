using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Zones;

[Mapper]
public static partial class ZoneInputMapper
{
    public static partial LastMile.TMS.Application.Zones.DTOs.CreateZoneDto ToDto(this CreateZoneInput input);

    public static partial LastMile.TMS.Application.Zones.DTOs.UpdateZoneDto ToDto(this UpdateZoneInput input);
}
