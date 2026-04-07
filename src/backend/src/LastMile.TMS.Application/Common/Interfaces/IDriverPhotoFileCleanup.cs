namespace LastMile.TMS.Application.Common.Interfaces;

/// <summary>
/// Deletes driver photos stored in object storage or under legacy wwwroot/uploads/drivers.
/// </summary>
public interface IDriverPhotoFileCleanup
{
    Task TryDeleteStoredPhotoAsync(string? photoUrl, CancellationToken cancellationToken = default);

    Task<int> DeleteOrphanDriverPhotosAsync(CancellationToken cancellationToken = default);
}
