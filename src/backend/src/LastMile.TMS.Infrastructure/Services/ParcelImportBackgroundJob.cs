using LastMile.TMS.Application.Parcels.Services;
using Microsoft.Extensions.Logging;

namespace LastMile.TMS.Infrastructure.Services;

public sealed class ParcelImportBackgroundJob(
    ParcelImportProcessor processor,
    ILogger<ParcelImportBackgroundJob> logger)
{
    public async Task ProcessAsync(Guid parcelImportId)
    {
        try
        {
            await processor.ProcessAsync(parcelImportId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Parcel import {ParcelImportId} failed.", parcelImportId);
            throw;
        }
    }
}
