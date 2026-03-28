using LastMile.TMS.Application.Users.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Users.Commands;

public sealed record CompletePasswordResetCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<UserActionResultDto>;
