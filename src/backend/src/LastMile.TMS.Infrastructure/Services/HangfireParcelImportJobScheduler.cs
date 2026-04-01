using Hangfire;
using LastMile.TMS.Application.Parcels.Services;

namespace LastMile.TMS.Infrastructure.Services;

public sealed class HangfireParcelImportJobScheduler(
    IBackgroundJobClient backgroundJobClient)
    : IParcelImportJobScheduler
{
    public Task ScheduleAsync(Guid parcelImportId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        backgroundJobClient.Enqueue<ParcelImportBackgroundJob>(job => job.ProcessAsync(parcelImportId));
        return Task.CompletedTask;
    }
}
