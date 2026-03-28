using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Routes;

[Mapper]
public static partial class RouteInputMapper
{
    public static partial LastMile.TMS.Application.Routes.DTOs.CreateRouteDto ToDto(this CreateRouteInput input);
}
