using MediatR;

namespace LastMile.TMS.Application.Depots.Commands;

public record DeleteDepotCommand(Guid Id) : IRequest<bool>;
