using LastMile.TMS.Api.Extensions;
using LastMile.TMS.Application.Common;
using LastMile.TMS.Application.Routes.Commands;
using LastMile.TMS.Application.Routes.DTOs;
using LastMile.TMS.Application.Routes.Queries;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastMile.TMS.Api.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize(Roles = "OperationsManager,Admin,Dispatcher")]
public class RoutesController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RouteDto>> Create([FromBody] CreateRouteDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateRouteCommand(dto);
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestFromBusinessRule(ex);
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<RouteDto>>> GetAll(
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] RouteStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRoutesQuery(vehicleId, status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RouteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRouteByIdQuery(id);
        var route = await mediator.Send(query, cancellationToken);

        if (route is null)
            return NotFound();

        return Ok(route);
    }

    [HttpGet("vehicle/{vehicleId:guid}/history")]
    public async Task<ActionResult<PaginatedResult<RouteDto>>> GetVehicleHistory(
        Guid vehicleId,
        [FromQuery] RouteStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRoutesQuery(vehicleId, status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}