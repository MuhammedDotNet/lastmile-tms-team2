using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Commands;

public record RegisterParcelCommand(RegisterParcelDto Dto) : IRequest<ParcelDto>;
