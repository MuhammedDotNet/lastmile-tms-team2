using LastMile.TMS.Application.Routes.Commands;
using LastMile.TMS.Application.Routes.DTOs;
using LastMile.TMS.Application.Vehicles.Commands;
using LastMile.TMS.Application.Vehicles.DTOs;
using HotChocolate.Authorization;
using MediatR;

namespace LastMile.TMS.Api.GraphQL;

public class Mutation
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<VehicleDto> CreateVehicle(
        CreateVehicleDto input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVehicleCommand(input);
        return await mediator.Send(command, cancellationToken);
    }

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<VehicleDto?> UpdateVehicle(
        Guid id,
        UpdateVehicleDto input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVehicleCommand(id, input);
        return await mediator.Send(command, cancellationToken);
    }

    [Authorize(Roles = new[] { "OperationsManager", "Admin" })]
    public async Task<bool> DeleteVehicle(
        Guid id,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVehicleCommand(id);
        return await mediator.Send(command, cancellationToken);
    }

    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    public async Task<RouteDto> CreateRoute(
        CreateRouteDto input,
        [Service] ISender mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateRouteCommand(input);
        return await mediator.Send(command, cancellationToken);
    }
}
