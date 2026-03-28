using LastMile.TMS.Application.Users.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Users.Queries;

public sealed record GetUserManagementLookupsQuery : IRequest<UserManagementLookupsDto>;
