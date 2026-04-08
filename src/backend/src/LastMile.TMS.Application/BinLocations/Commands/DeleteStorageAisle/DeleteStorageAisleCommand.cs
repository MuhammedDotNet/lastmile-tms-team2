using MediatR;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed record DeleteStorageAisleCommand(Guid Id) : IRequest<bool>;
