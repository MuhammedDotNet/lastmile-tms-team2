using LastMile.TMS.Application.Zones.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Zones.Commands;

public record CreateZoneCommand(CreateZoneDto Dto) : IRequest<Zone>;
