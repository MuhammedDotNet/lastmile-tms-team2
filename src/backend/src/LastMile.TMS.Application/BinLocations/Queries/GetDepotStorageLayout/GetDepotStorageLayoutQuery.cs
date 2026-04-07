using LastMile.TMS.Application.BinLocations.DTOs;
using MediatR;

namespace LastMile.TMS.Application.BinLocations.Queries;

public sealed record GetDepotStorageLayoutQuery(Guid DepotId) : IRequest<DepotStorageLayoutDto?>;
