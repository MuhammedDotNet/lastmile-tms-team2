namespace LastMile.TMS.Application.Common.Models;

public sealed record FileStorageReadResult(
    Stream Content,
    string? ContentType,
    long? Length);
