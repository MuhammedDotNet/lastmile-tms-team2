using MediatR;

namespace LastMile.TMS.Application.Drivers.Commands;

public record DeleteDriverCommand(Guid Id) : IRequest<bool>;
