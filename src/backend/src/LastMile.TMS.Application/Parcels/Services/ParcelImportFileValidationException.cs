namespace LastMile.TMS.Application.Parcels.Services;

public sealed class ParcelImportFileValidationException(string message) : Exception(message);
