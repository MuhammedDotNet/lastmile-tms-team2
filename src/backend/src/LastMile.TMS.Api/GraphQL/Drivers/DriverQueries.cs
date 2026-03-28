using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Drivers.Reads;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Drivers;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class DriverQueries
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    [UseProjection]
    public IQueryable<Driver> GetDrivers(
        Guid? depotId = null,
        [Service] IDriverReadService readService = null!) =>
        readService.GetDrivers(depotId);
}
