using LastMile.TMS.Application.Drivers.DTOs;
using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Drivers;

[Mapper]
public static partial class DriverInputMapper
{
    public static partial CreateDriverDto ToDto(this CreateDriverInput input);

    public static partial UpdateDriverDto ToDto(this UpdateDriverInput input);

    private static partial CreateDriverAvailabilityDto ToAvailDto(this CreateDriverAvailabilityInput input);

    private static partial UpdateDriverAvailabilityDto ToAvailDto(this UpdateDriverAvailabilityInput input);
}
