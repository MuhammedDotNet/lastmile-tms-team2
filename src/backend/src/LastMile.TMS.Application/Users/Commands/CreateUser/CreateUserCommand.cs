using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Users.Commands;

public sealed record CreateUserCommand(CreateUserDto Dto) : IRequest<ApplicationUser>;
