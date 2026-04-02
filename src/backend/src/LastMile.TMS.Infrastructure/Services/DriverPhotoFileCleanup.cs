using System.Text.RegularExpressions;
using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IoPath = System.IO.Path;

namespace LastMile.TMS.Infrastructure.Services;

/// <summary>
/// Removes files under wwwroot/uploads/drivers when they are no longer referenced (see API upload endpoint).
/// </summary>
public sealed class DriverPhotoFileCleanup : IDriverPhotoFileCleanup
{
    private const string UploadsDriversPrefix = "/uploads/drivers/";

    private static readonly Regex StoredFileNameRegex = new(
        @"^[a-f0-9]{32}\.(jpg|jpeg|png|webp|gif)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IWebHostEnvironment _env;
    private readonly IAppDbContext _dbContext;

    public DriverPhotoFileCleanup(IWebHostEnvironment env, IAppDbContext dbContext)
    {
        _env = env;
        _dbContext = dbContext;
    }

    public void TryDeleteStoredPhoto(string? photoUrl)
    {
        var fileName = TryGetStoredFileName(photoUrl);
        if (fileName is null)
            return;

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

    public async Task<int> DeleteOrphanDriverPhotosAsync(CancellationToken cancellationToken = default)
    {
        var urls = await _dbContext.Drivers
            .AsNoTracking()
            .Where(d => d.PhotoUrl != null && d.PhotoUrl != "")
            .Select(d => d.PhotoUrl!)
            .ToListAsync(cancellationToken);

        var referencedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in urls)
        {
            var name = TryGetStoredFileName(url);
            if (name is not null)
                referencedNames.Add(name);
        }

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
            if (!StoredFileNameRegex.IsMatch(fileName))
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

    private static string? TryGetStoredFileName(string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return null;

        var path = photoUrl.Trim();
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
                return null;
            path = uri.AbsolutePath;
        }

        if (!path.StartsWith(UploadsDriversPrefix, StringComparison.Ordinal))
            return null;

        var fileName = path[UploadsDriversPrefix.Length..];
        if (fileName.Length == 0 || fileName.AsSpan().IndexOfAny(IoPath.GetInvalidFileNameChars()) >= 0)
            return null;

        if (!StoredFileNameRegex.IsMatch(fileName))
            return null;

        return fileName;
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
        if (!fullPath.StartsWith(prefix, comparison))
            return false;

        return true;
    }
}
