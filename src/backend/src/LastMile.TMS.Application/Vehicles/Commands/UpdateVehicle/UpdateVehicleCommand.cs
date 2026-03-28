using LastMile.TMS.Application.Vehicles.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Vehicles.Commands;

public record UpdateVehicleCommand(Guid Id, UpdateVehicleDto Dto) : IRequest<Vehicle?>;
