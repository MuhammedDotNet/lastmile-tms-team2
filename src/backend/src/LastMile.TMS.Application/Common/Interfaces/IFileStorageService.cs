using LastMile.TMS.Application.Common.Models;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task UploadAsync(
        string key,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<FileStorageReadResult> OpenReadAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListKeysAsync(
        string prefix,
        CancellationToken cancellationToken = default);
}
