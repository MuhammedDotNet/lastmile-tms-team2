using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class ParcelImportRowFailure : BaseEntity
{
    public Guid ParcelImportId { get; set; }
    public int RowNumber { get; set; }
    public string OriginalRowValues { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public ParcelImport ParcelImport { get; set; } = null!;
}
