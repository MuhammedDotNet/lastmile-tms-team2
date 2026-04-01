namespace LastMile.TMS.Application.Parcels.Services;

public interface IParcelImportJobScheduler
{
    Task ScheduleAsync(Guid parcelImportId, CancellationToken cancellationToken = default);
}
