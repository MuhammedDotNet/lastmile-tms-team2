using LastMile.TMS.Application.BinLocations.DTOs;
using MediatR;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed record CreateStorageZoneCommand(CreateStorageZoneDto Dto) : IRequest<StorageZoneResultDto>;
