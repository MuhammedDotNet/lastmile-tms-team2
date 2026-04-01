using LastMile.TMS.Application.Parcels.DTOs;

namespace LastMile.TMS.Application.Parcels.Services;

public interface IParcelRegistrationService
{
    Task<ParcelDto> RegisterAsync(
        RegisterParcelDto dto,
        CancellationToken cancellationToken = default,
        Guid? parcelImportId = null,
        string? actorOverride = null);
}
