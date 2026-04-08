using MediatR;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed record DeleteBinLocationCommand(Guid Id) : IRequest<bool>;
