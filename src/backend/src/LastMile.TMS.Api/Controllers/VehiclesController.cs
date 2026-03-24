using LastMile.TMS.Api.Extensions;
using LastMile.TMS.Application.Common;
using LastMile.TMS.Application.Vehicles.Commands;
using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Application.Vehicles.Queries;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastMile.TMS.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "OperationsManager,Admin")]
    public async Task<ActionResult<VehicleDto>> Create([FromBody] CreateVehicleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateVehicleCommand(dto);
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestFromBusinessRule(ex);
        }
    }

    [HttpGet]
    [Authorize(Roles = "OperationsManager,Admin,Dispatcher")]
    public async Task<ActionResult<PaginatedResult<VehicleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] VehicleStatus? status = null,
        [FromQuery] Guid? depotId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVehiclesQuery(page, pageSize, status, depotId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "OperationsManager,Admin,Dispatcher")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetVehicleByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "OperationsManager,Admin")]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, [FromBody] UpdateVehicleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateVehicleCommand(id, dto);
            var result = await mediator.Send(command, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestFromBusinessRule(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "OperationsManager,Admin")]
    public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteVehicleCommand(id);
            var result = await mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            return Ok(true);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestFromBusinessRule(ex);
        }
    }
}