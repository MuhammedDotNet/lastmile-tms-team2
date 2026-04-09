using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Routes.Commands;

public sealed record CancelRouteCommand(Guid Id, DTOs.CancelRouteDto Dto) : IRequest<Route?>;
