using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed record CreateParcelImportCommand(CreateParcelImportDto Dto) : IRequest<Guid>;
