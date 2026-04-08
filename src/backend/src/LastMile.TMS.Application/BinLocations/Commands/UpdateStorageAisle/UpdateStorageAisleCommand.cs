using LastMile.TMS.Application.BinLocations.DTOs;
using MediatR;

namespace LastMile.TMS.Application.BinLocations.Commands;

public sealed record UpdateStorageAisleCommand(Guid Id, UpdateStorageAisleDto Dto) : IRequest<StorageAisleResultDto?>;
