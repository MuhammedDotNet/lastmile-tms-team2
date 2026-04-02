using LastMile.TMS.Application.Drivers.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Drivers.Commands;

public record UpdateDriverCommand(Guid Id, UpdateDriverDto Dto) : IRequest<Domain.Entities.Driver>;
