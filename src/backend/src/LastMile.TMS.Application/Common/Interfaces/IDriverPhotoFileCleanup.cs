namespace LastMile.TMS.Application.Common.Interfaces;

/// <summary>
/// Deletes driver photos stored under wwwroot/uploads/drivers (single file or orphan sweep).
/// </summary>
public interface IDriverPhotoFileCleanup
{
    void TryDeleteStoredPhoto(string? photoUrl);

    Task<int> DeleteOrphanDriverPhotosAsync(CancellationToken cancellationToken = default);
}
