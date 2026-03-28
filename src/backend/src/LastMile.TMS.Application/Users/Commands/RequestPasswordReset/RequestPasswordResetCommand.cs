using LastMile.TMS.Application.Users.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Users.Commands;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<UserActionResultDto>;
