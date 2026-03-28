using LastMile.TMS.Application.Depots.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Depots.Commands;

public record UpdateDepotCommand(Guid Id, UpdateDepotDto Dto) : IRequest<Depot?>;
