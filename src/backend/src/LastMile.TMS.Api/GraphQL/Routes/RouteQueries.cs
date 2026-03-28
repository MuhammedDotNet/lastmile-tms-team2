using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Routes.Reads;
using RouteEntity = LastMile.TMS.Domain.Entities.Route;

namespace LastMile.TMS.Api.GraphQL.Routes;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class RouteQueries
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    [UseProjection]
    [UseSorting(typeof(RouteSortInputType))]
    [UseFiltering(typeof(RouteFilterInputType))]
    public IQueryable<RouteEntity> GetRoutes(
        [Service] IRouteReadService readService = null!) =>
        readService.GetRoutes();
}
