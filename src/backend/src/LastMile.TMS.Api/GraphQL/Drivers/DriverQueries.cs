using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Drivers.Queries;
using LastMile.TMS.Application.Drivers.Reads;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Drivers;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class DriverQueries
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    [UseProjection]
    [UseSorting(typeof(DriverSortInputType))]
    [UseFiltering(typeof(DriverFilterInputType))]
    public IQueryable<Driver> GetDrivers(
        [Service] IDriverReadService readService = null!) =>
        readService.GetDrivers();

    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    public Task<Driver?> GetDriver(
        Guid id,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new GetDriverQuery(id), cancellationToken);
}
