using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Drivers.Commands;

public record CreateDriverCommand(CreateDriverDto Dto) : IRequest<Driver>;
