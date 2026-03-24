using LastMile.TMS.Application.Vehicles.DTOs;

namespace LastMile.TMS.Api.GraphQL.Types;

/// <summary>
/// Explicit GraphQL object type so all <see cref="VehicleDto"/> properties (including
/// <see cref="VehicleDto.TotalRoutes"/>) are bound to the schema. Inference alone can miss
/// new fields when the type is reused across resolvers.
/// </summary>
public sealed class VehicleDtoType : ObjectType<VehicleDto>
{
    protected override void Configure(IObjectTypeDescriptor<VehicleDto> descriptor)
    {
        descriptor.BindFieldsImplicitly();
    }
}
