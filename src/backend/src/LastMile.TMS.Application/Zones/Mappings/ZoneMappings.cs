using LastMile.TMS.Application.Zones.DTOs;
using LastMile.TMS.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Application.Zones.Mappings;

[Mapper]
public static partial class ZoneMappings
{
    [MapperIgnoreTarget(nameof(Zone.Boundary))]
    [MapperIgnoreTarget(nameof(Zone.Depot))]
    [MapperIgnoreTarget(nameof(Zone.Parcels))]
    [MapperIgnoreTarget(nameof(Zone.Drivers))]
    [MapperIgnoreTarget(nameof(Zone.CreatedAt))]
    [MapperIgnoreTarget(nameof(Zone.CreatedBy))]
    [MapperIgnoreTarget(nameof(Zone.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Zone.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(Zone.Id))]
    [MapperIgnoreSource(nameof(CreateZoneDto.GeoJson))]
    [MapperIgnoreSource(nameof(CreateZoneDto.Coordinates))]
    [MapperIgnoreSource(nameof(CreateZoneDto.BoundaryWkt))]
    public static partial Zone ToEntity(this CreateZoneDto dto);

    [MapperIgnoreTarget(nameof(Zone.Boundary))]
    [MapperIgnoreTarget(nameof(Zone.Depot))]
    [MapperIgnoreTarget(nameof(Zone.Parcels))]
    [MapperIgnoreTarget(nameof(Zone.Drivers))]
    [MapperIgnoreTarget(nameof(Zone.CreatedAt))]
    [MapperIgnoreTarget(nameof(Zone.CreatedBy))]
    [MapperIgnoreTarget(nameof(Zone.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(Zone.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(Zone.Id))]
    [MapperIgnoreSource(nameof(UpdateZoneDto.GeoJson))]
    [MapperIgnoreSource(nameof(UpdateZoneDto.Coordinates))]
    [MapperIgnoreSource(nameof(UpdateZoneDto.BoundaryWkt))]
    public static partial void UpdateEntity(this UpdateZoneDto dto, [MappingTarget] Zone zone);
}
