namespace LastMile.TMS.Application.Parcels.Services;

public interface IParcelImportFileParser
{
    Task<ParcelImportParsedFile> ParseAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default);
}
