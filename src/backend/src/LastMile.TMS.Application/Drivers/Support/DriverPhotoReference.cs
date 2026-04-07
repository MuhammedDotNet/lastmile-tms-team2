using System.Text.RegularExpressions;

namespace LastMile.TMS.Application.Drivers.Support;

public enum DriverPhotoLocation
{
    LegacyFileSystem,
    ObjectStorage,
}

public readonly record struct ParsedDriverPhotoReference(
    DriverPhotoLocation Location,
    string FileName)
{
    public string StorageKey => $"{DriverPhotoReference.StoragePrefix}{FileName}";

    public string RelativeUrl => Location switch
    {
        DriverPhotoLocation.LegacyFileSystem => $"{DriverPhotoReference.LegacyUrlPrefix}{FileName}",
        DriverPhotoLocation.ObjectStorage => $"{DriverPhotoReference.ProxyUrlPrefix}{FileName}",
        _ => throw new InvalidOperationException($"Unsupported driver photo location '{Location}'."),
    };
}

public static class DriverPhotoReference
{
    public const string LegacyUrlPrefix = "/uploads/drivers/";
    public const string ProxyUrlPrefix = "/api/drivers/photo/";
    public const string StoragePrefix = "drivers/";

    private static readonly Regex FileNameRegex = new(
        @"^[a-f0-9]{32}\.(jpg|jpeg|png|webp|gif)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".gif",
    ];

    public static bool IsAllowedExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return AllowedExtensions.Contains(extension.Trim().ToLowerInvariant());
    }

    public static string CreateFileName(string extension)
    {
        var normalizedExtension = extension.Trim().ToLowerInvariant();
        if (!IsAllowedExtension(normalizedExtension))
        {
            throw new InvalidOperationException($"Unsupported driver photo extension '{extension}'.");
        }

        return $"{Guid.NewGuid():N}{normalizedExtension}";
    }

    public static string BuildObjectStorageUrl(string fileName)
    {
        EnsureValidFileName(fileName);
        return $"{ProxyUrlPrefix}{fileName}";
    }

    public static string BuildStorageKey(string fileName)
    {
        EnsureValidFileName(fileName);
        return $"{StoragePrefix}{fileName}";
    }

    public static bool IsSupportedPhotoUrl(string? photoUrl) =>
        photoUrl is null || TryParse(photoUrl, out _);

    public static string? NormalizeStoredUrl(string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return null;
        }

        if (!TryParse(photoUrl, out var reference))
        {
            throw new InvalidOperationException($"Unsupported driver photo URL '{photoUrl}'.");
        }

        return reference.RelativeUrl;
    }

    public static bool TryParse(string? photoUrl, out ParsedDriverPhotoReference reference)
    {
        reference = default;
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return false;
        }

        var path = photoUrl.Trim();
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                return false;
            }

            path = uri.AbsolutePath;
        }

        if (!path.StartsWith('/'))
        {
            path = $"/{path}";
        }

        if (path.StartsWith(LegacyUrlPrefix, StringComparison.Ordinal))
        {
            var fileName = path[LegacyUrlPrefix.Length..];
            if (!IsValidFileName(fileName))
            {
                return false;
            }

            reference = new ParsedDriverPhotoReference(DriverPhotoLocation.LegacyFileSystem, fileName);
            return true;
        }

        if (path.StartsWith(ProxyUrlPrefix, StringComparison.Ordinal))
        {
            var fileName = path[ProxyUrlPrefix.Length..];
            if (!IsValidFileName(fileName))
            {
                return false;
            }

            reference = new ParsedDriverPhotoReference(DriverPhotoLocation.ObjectStorage, fileName);
            return true;
        }

        return false;
    }

    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        return FileNameRegex.IsMatch(fileName)
            && fileName.AsSpan().IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    private static void EnsureValidFileName(string fileName)
    {
        if (!IsValidFileName(fileName))
        {
            throw new InvalidOperationException($"Unsupported driver photo file name '{fileName}'.");
        }
    }
}
