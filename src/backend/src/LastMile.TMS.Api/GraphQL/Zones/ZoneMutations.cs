using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Zones.Commands;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Zones;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class ZoneMutations
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<Zone> CreateZone(
        CreateZoneInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new CreateZoneCommand(input.ToDto()), cancellationToken);

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<Zone?> UpdateZone(
        Guid id,
        UpdateZoneInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new UpdateZoneCommand(id, input.ToDto()), cancellationToken);

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<bool> DeleteZone(
        Guid id,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new DeleteZoneCommand(id), cancellationToken);
}
