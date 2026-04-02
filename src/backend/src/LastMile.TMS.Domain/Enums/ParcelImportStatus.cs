namespace LastMile.TMS.Domain.Enums;

public enum ParcelImportStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4,
}
