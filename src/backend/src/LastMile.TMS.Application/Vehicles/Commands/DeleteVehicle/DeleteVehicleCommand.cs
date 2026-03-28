using MediatR;

namespace LastMile.TMS.Application.Vehicles.Commands;

public record DeleteVehicleCommand(Guid Id) : IRequest<bool>;
