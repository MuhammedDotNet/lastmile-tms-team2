using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Drivers.Commands;
using LastMile.TMS.Application.Drivers.Queries;
using MediatR;
using DriverEntity = LastMile.TMS.Domain.Entities.Driver;

namespace LastMile.TMS.Api.GraphQL.Drivers;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class DriverMutations
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<DriverEntity> CreateDriver(
        CreateDriverInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new CreateDriverCommand(input.ToDto()), cancellationToken);
    }

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<DriverEntity> UpdateDriver(
        Guid id,
        UpdateDriverInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new UpdateDriverCommand(id, input.ToDto()), cancellationToken);
    }

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<bool> DeleteDriver(
        Guid id,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new DeleteDriverCommand(id), cancellationToken);
    }
}
