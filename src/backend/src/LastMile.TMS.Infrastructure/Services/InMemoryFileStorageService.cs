using System.Collections.Concurrent;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Common.Models;

namespace LastMile.TMS.Infrastructure.Services;

public sealed class InMemoryFileStorageService : IFileStorageService
{
    private readonly ConcurrentDictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);

    public async Task UploadAsync(
        string key,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _objects[key] = new StoredObject(buffer.ToArray(), contentType);
    }

    public Task<FileStorageReadResult> OpenReadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);

        if (!_objects.TryGetValue(key, out var storedObject))
        {
            throw new FileNotFoundException($"Stored object '{key}' was not found.", key);
        }

        var stream = new MemoryStream(storedObject.Content, writable: false);
        return Task.FromResult(new FileStorageReadResult(stream, storedObject.ContentType, storedObject.Content.LongLength));
    }

    public Task DeleteIfExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);
        _objects.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListKeysAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        prefix ??= string.Empty;

        IReadOnlyList<string> keys = _objects.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult(keys);
    }

    public void Clear() => _objects.Clear();

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Storage key is required.", nameof(key));
        }
    }

    private sealed record StoredObject(byte[] Content, string? ContentType);
}
