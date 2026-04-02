using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Application.Drivers.Mappings;

[Mapper]
public static partial class DriverMapping
{
    [MapperIgnoreSource(nameof(CreateDriverDto.AvailabilitySchedule))]
    [MapperIgnoreTarget(nameof(Driver.Id))]
    [MapperIgnoreTarget(nameof(Driver.Zone))]
    [MapperIgnoreTarget(nameof(Driver.Depot))]
    [MapperIgnoreTarget(nameof(Driver.User))]
    [MapperIgnoreTarget(nameof(Driver.AvailabilitySchedule))]
    [MapperIgnoreTarget(nameof(Driver.CreatedAt))]
    [MapperIgnoreTarget(nameof(Driver.CreatedBy))]
    [MapperIgnoreTarget(nameof(Driver.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Driver.LastModifiedBy))]
    public static partial Driver ToEntity(this CreateDriverDto dto);

    [MapperIgnoreSource(nameof(UpdateDriverDto.AvailabilitySchedule))]
    [MapperIgnoreTarget(nameof(Driver.Id))]
    [MapperIgnoreTarget(nameof(Driver.Zone))]
    [MapperIgnoreTarget(nameof(Driver.Depot))]
    [MapperIgnoreTarget(nameof(Driver.User))]
    [MapperIgnoreTarget(nameof(Driver.AvailabilitySchedule))]
    [MapperIgnoreTarget(nameof(Driver.CreatedAt))]
    [MapperIgnoreTarget(nameof(Driver.CreatedBy))]
    [MapperIgnoreTarget(nameof(Driver.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Driver.LastModifiedBy))]
    public static partial void UpdateEntity(this UpdateDriverDto dto, Driver driver);

    [MapperIgnoreTarget(nameof(DriverAvailability.Id))]
    [MapperIgnoreTarget(nameof(DriverAvailability.DriverId))]
    [MapperIgnoreTarget(nameof(DriverAvailability.Driver))]
    [MapperIgnoreTarget(nameof(DriverAvailability.CreatedAt))]
    [MapperIgnoreTarget(nameof(DriverAvailability.CreatedBy))]
    [MapperIgnoreTarget(nameof(DriverAvailability.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(DriverAvailability.LastModifiedBy))]
    public static partial DriverAvailability ToEntity(this CreateDriverAvailabilityDto dto);

    [MapperIgnoreSource(nameof(DriverAvailability.DriverId))]
    [MapperIgnoreSource(nameof(DriverAvailability.Driver))]
    [MapperIgnoreSource(nameof(DriverAvailability.CreatedAt))]
    [MapperIgnoreSource(nameof(DriverAvailability.CreatedBy))]
    [MapperIgnoreSource(nameof(DriverAvailability.LastModifiedAt))]
    [MapperIgnoreSource(nameof(DriverAvailability.LastModifiedBy))]
    public static partial DriverAvailabilityDto ToDto(this DriverAvailability entity);
}
