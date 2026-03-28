using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Application.Vehicles.Mappings;

[Mapper]
public static partial class VehicleMappings
{
    [MapperIgnoreTarget(nameof(Vehicle.Depot))]
    [MapperIgnoreTarget(nameof(Vehicle.CreatedAt))]
    [MapperIgnoreTarget(nameof(Vehicle.CreatedBy))]
    [MapperIgnoreTarget(nameof(Vehicle.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Vehicle.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(Vehicle.Id))]
    public static partial Vehicle ToEntity(this CreateVehicleDto dto);

    [MapperIgnoreTarget(nameof(Vehicle.Depot))]
    [MapperIgnoreTarget(nameof(Vehicle.CreatedAt))]
    [MapperIgnoreTarget(nameof(Vehicle.CreatedBy))]
    [MapperIgnoreTarget(nameof(Vehicle.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Vehicle.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(Vehicle.Id))]
    public static partial void UpdateEntity(this UpdateVehicleDto dto, [MappingTarget] Vehicle vehicle);
}
