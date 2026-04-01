using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Services;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class RegisterParcelCommandHandler(
    IParcelRegistrationService registrationService)
    : IRequestHandler<RegisterParcelCommand, ParcelDto>
{
    public Task<ParcelDto> Handle(RegisterParcelCommand request, CancellationToken cancellationToken)
        => registrationService.RegisterAsync(request.Dto, cancellationToken);
}
