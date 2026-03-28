using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Vehicles;

[Mapper]
public static partial class VehicleInputMapper
{
    public static partial LastMile.TMS.Application.Vehicles.DTOs.CreateVehicleDto ToDto(this CreateVehicleInput input);

    public static partial LastMile.TMS.Application.Vehicles.DTOs.UpdateVehicleDto ToDto(this UpdateVehicleInput input);
}
