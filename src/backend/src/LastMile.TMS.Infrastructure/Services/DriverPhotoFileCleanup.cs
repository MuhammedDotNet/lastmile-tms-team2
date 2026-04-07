using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IoPath = System.IO.Path;

namespace LastMile.TMS.Infrastructure.Services;

/// <summary>
/// Removes driver photos that are no longer referenced, from object storage or the legacy uploads folder.
/// </summary>
public sealed class DriverPhotoFileCleanup : IDriverPhotoFileCleanup
{
    private readonly IWebHostEnvironment _env;
    private readonly IAppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public DriverPhotoFileCleanup(
        IWebHostEnvironment env,
        IAppDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _env = env;
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task TryDeleteStoredPhotoAsync(
        string? photoUrl,
        CancellationToken cancellationToken = default)
    {
        if (!DriverPhotoReference.TryParse(photoUrl, out var photoReference))
            return;

        if (photoReference.Location == DriverPhotoLocation.ObjectStorage)
        {
            await _fileStorageService.DeleteIfExistsAsync(photoReference.StorageKey, cancellationToken);
            return;
        }

        TryDeleteLegacyFile(photoReference.FileName);
    }

    public async Task<int> DeleteOrphanDriverPhotosAsync(CancellationToken cancellationToken = default)
    {
        var urls = await _dbContext.Drivers
            .AsNoTracking()
            .Where(d => d.PhotoUrl != null && d.PhotoUrl != "")
            .Select(d => d.PhotoUrl!)
            .ToListAsync(cancellationToken);

        var referencedLegacyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var referencedObjectKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var url in urls)
        {
            if (!DriverPhotoReference.TryParse(url, out var photoReference))
                continue;

            if (photoReference.Location == DriverPhotoLocation.ObjectStorage)
            {
                referencedObjectKeys.Add(photoReference.StorageKey);
            }
            else
            {
                referencedLegacyNames.Add(photoReference.FileName);
            }
        }

        var deleted = await DeleteOrphanObjectStoragePhotosAsync(referencedObjectKeys, cancellationToken);
        deleted += DeleteOrphanLegacyPhotos(referencedLegacyNames);
        return deleted;
    }

    private async Task<int> DeleteOrphanObjectStoragePhotosAsync(
        HashSet<string> referencedObjectKeys,
        CancellationToken cancellationToken)
    {
        var deleted = 0;
        var keys = await _fileStorageService.ListKeysAsync(DriverPhotoReference.StoragePrefix, cancellationToken);
        foreach (var key in keys)
        {
            if (referencedObjectKeys.Contains(key))
            {
                continue;
            }

            await _fileStorageService.DeleteIfExistsAsync(key, cancellationToken);
            deleted++;
        }

        return deleted;
    }

    private int DeleteOrphanLegacyPhotos(HashSet<string> referencedNames)
    {
        var driversDir = IoPath.Combine(GetWebRootPath(), "uploads", "drivers");
        if (!Directory.Exists(driversDir))
            return 0;

        var root = IoPath.GetFullPath(driversDir);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = root.TrimEnd(IoPath.DirectorySeparatorChar) + IoPath.DirectorySeparatorChar;

        var deleted = 0;
        foreach (var fullPath in Directory.EnumerateFiles(driversDir))
        {
            var fileName = IoPath.GetFileName(fullPath);
            if (!DriverPhotoReference.IsValidFileName(fileName))
                continue;

            if (referencedNames.Contains(fileName))
                continue;

            var normalized = IoPath.GetFullPath(fullPath);
            if (!normalized.StartsWith(prefix, comparison))
                continue;

            try
            {
                File.Delete(fullPath);
                deleted++;
            }
            catch (IOException)
            {
                // Best-effort cleanup.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return deleted;
    }

    private string GetWebRootPath()
    {
        return string.IsNullOrEmpty(_env.WebRootPath)
            ? IoPath.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;
    }

    private void TryDeleteLegacyFile(string fileName)
    {
        if (!TryResolvePhysicalPath(fileName, out var fullPath))
            return;

        if (!File.Exists(fullPath))
            return;

        try
        {
            File.Delete(fullPath);
        }
        catch (IOException)
        {
            // Best-effort cleanup; do not fail the business operation.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private bool TryResolvePhysicalPath(string fileName, out string fullPath)
    {
        fullPath = "";

        var driversDir = IoPath.Combine(GetWebRootPath(), "uploads", "drivers");
        var root = IoPath.GetFullPath(driversDir);
        fullPath = IoPath.GetFullPath(IoPath.Combine(driversDir, fileName));

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = root.TrimEnd(IoPath.DirectorySeparatorChar) + IoPath.DirectorySeparatorChar;
        return fullPath.StartsWith(prefix, comparison);
    }
}
