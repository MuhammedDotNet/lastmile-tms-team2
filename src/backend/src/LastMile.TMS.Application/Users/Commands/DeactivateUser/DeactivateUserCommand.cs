using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Users.Commands;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest<ApplicationUser>;
