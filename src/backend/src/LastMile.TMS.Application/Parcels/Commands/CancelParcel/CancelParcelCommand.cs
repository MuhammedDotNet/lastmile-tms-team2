using LastMile.TMS.Application.Parcels.DTOs;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed record CancelParcelCommand(Guid ParcelId, string Reason) : IRequest<ParcelDetailDto?>;
