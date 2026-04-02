using LastMile.TMS.Application.Parcels.Services;

namespace LastMile.TMS.Infrastructure.Services;

public sealed class ImmediateParcelImportJobScheduler(
    ParcelImportBackgroundJob backgroundJob)
    : IParcelImportJobScheduler
{
    public Task ScheduleAsync(Guid parcelImportId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return backgroundJob.ProcessAsync(parcelImportId);
    }
}
