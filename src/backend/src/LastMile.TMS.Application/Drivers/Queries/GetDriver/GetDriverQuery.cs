using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Drivers.Queries;

public record GetDriverQuery(Guid Id) : IRequest<Driver?>;
