using MediatR;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed record DeleteStorageZoneCommand(Guid Id) : IRequest<bool>;
