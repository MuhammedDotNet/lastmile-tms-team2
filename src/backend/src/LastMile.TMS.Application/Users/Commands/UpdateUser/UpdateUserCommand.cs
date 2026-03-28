using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Users.Commands;

public sealed record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<ApplicationUser>;
