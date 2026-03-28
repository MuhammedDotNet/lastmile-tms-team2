using LastMile.TMS.Application.Zones.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Zones.Commands;

public record UpdateZoneCommand(Guid Id, UpdateZoneDto Dto) : IRequest<Zone?>;
