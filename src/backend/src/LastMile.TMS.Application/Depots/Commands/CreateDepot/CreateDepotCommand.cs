using LastMile.TMS.Application.Depots.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Depots.Commands;

public record CreateDepotCommand(CreateDepotDto Dto) : IRequest<Depot>;
