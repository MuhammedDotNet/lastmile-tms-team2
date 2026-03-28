using HotChocolate;
using HotChocolate.Authorization;
using LastMile.TMS.Application.Vehicles.Commands;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Vehicles;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class VehicleMutations
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<Vehicle> CreateVehicle(
        CreateVehicleInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new CreateVehicleCommand(input.ToDto()), cancellationToken);

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<Vehicle?> UpdateVehicle(
        Guid id,
        UpdateVehicleInput input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new UpdateVehicleCommand(id, input.ToDto()), cancellationToken);

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public Task<bool> DeleteVehicle(
        Guid id,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new DeleteVehicleCommand(id), cancellationToken);
}
